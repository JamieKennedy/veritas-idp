using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Veritas.Tooling.DbMigrator;

/// <summary>
/// Provides helper methods for applying module-owned EF Core migrations.
/// </summary>
public static class Utilities
{
    private const int AdvisoryLockKey = 0x56455249; // VERI

    /// <summary>
    /// Applies pending EF Core migrations for the specified module DbContext.
    /// </summary>
    /// <param name="services">The root service provider used to resolve the scoped DbContext.</param>
    /// <param name="cancellationToken">A token that cancels readiness checks and migration execution.</param>
    /// <typeparam name="TContext">The module DbContext type whose migrations should be applied.</typeparam>
    /// <returns>A task that completes when all pending migrations have been applied.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services" /> is <see langword="null" />.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the database cannot be reached or migrations cannot be applied.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken" /> is canceled.</exception>
    public static async Task MigrateAsync<TContext>(
        IServiceProvider services,
        CancellationToken cancellationToken = default)
        where TContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);

        await using var scope = services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("Veritas.Tooling.DbMigrator.Migrations");
        var options = scope.ServiceProvider.GetRequiredService<IOptions<MigrationOptions>>().Value;
        ValidateOptions(options);

        var contextName = typeof(TContext).FullName ?? typeof(TContext).Name;
        await WaitForDatabaseAsync(dbContext, logger, options, contextName, cancellationToken);

        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        try
        {
            await AcquireMigrationLockAsync(dbContext, logger, contextName, cancellationToken);

            var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync(cancellationToken)).ToArray();
            logger.LogInformation(
                "Preparing migrations for {DbContext}. Pending migration count: {PendingMigrationCount}. Pending migrations: {PendingMigrations}.",
                contextName,
                pendingMigrations.Length,
                pendingMigrations);

            if (pendingMigrations.Length == 0)
            {
                logger.LogInformation("No pending migrations for {DbContext}.", contextName);
                return;
            }

            logger.LogInformation("Applying migrations for {DbContext}.", contextName);
            await dbContext.Database.MigrateAsync(cancellationToken);
            logger.LogInformation("Applied migrations for {DbContext}.", contextName);
        }
        finally
        {
            await ReleaseMigrationLockAsync(dbContext, logger, contextName);
            await dbContext.Database.CloseConnectionAsync();
        }
    }

    /// <summary>
    /// Validates migration runtime options before using them for retry decisions.
    /// </summary>
    /// <param name="options">The migration options loaded from configuration.</param>
    /// <exception cref="InvalidOperationException">Thrown when an option value is outside the supported range.</exception>
    private static void ValidateOptions(MigrationOptions options)
    {
        if (options.MaxConnectAttempts < 1)
        {
            throw new InvalidOperationException(
                $"{MigrationOptions.SectionName}:{nameof(MigrationOptions.MaxConnectAttempts)} must be greater than zero.");
        }

        if (options.ConnectRetryDelay < TimeSpan.Zero)
        {
            throw new InvalidOperationException(
                $"{MigrationOptions.SectionName}:{nameof(MigrationOptions.ConnectRetryDelay)} must not be negative.");
        }
    }

    /// <summary>
    /// Waits until the database is reachable, retrying transient startup failures.
    /// </summary>
    /// <param name="dbContext">The DbContext used to test database connectivity.</param>
    /// <param name="logger">The logger that records retry progress.</param>
    /// <param name="contextName">The display name of the DbContext waiting for connectivity.</param>
    /// <param name="cancellationToken">A token that cancels retry attempts.</param>
    /// <typeparam name="TContext">The module DbContext type waiting for database connectivity.</typeparam>
    /// <returns>A task that completes once the database can be reached.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the database is not reachable after all retry attempts.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken" /> is canceled.</exception>
    private static async Task WaitForDatabaseAsync<TContext>(
        TContext dbContext,
        ILogger logger,
        MigrationOptions options,
        string contextName,
        CancellationToken cancellationToken)
        where TContext : DbContext
    {
        for (var attempt = 1; attempt <= options.MaxConnectAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                if (await dbContext.Database.CanConnectAsync(cancellationToken))
                {
                    logger.LogInformation(
                        "Database connection is available for {DbContext} after {AttemptCount} attempt(s).",
                        contextName,
                        attempt);
                    return;
                }

                if (attempt < options.MaxConnectAttempts)
                {
                    logger.LogWarning(
                        "Database connection attempt {AttemptCount}/{MaxAttemptCount} was not available for {DbContext}.",
                        attempt,
                        options.MaxConnectAttempts,
                        contextName);
                }
            }
            catch (Exception exception) when (attempt < options.MaxConnectAttempts && !cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning(
                    exception,
                    "Database connection attempt {AttemptCount}/{MaxAttemptCount} failed for {DbContext}.",
                    attempt,
                    options.MaxConnectAttempts,
                    contextName);
            }

            if (attempt < options.MaxConnectAttempts)
            {
                await Task.Delay(options.ConnectRetryDelay, cancellationToken);
            }
        }

        throw new InvalidOperationException(
            $"Database connection was not available for {contextName} after {options.MaxConnectAttempts} attempts.");
    }

    /// <summary>
    /// Acquires a PostgreSQL advisory lock on the current database connection to serialize migration execution.
    /// </summary>
    /// <param name="dbContext">The DbContext whose open connection receives the lock.</param>
    /// <param name="logger">The logger that records lock acquisition progress.</param>
    /// <param name="contextName">The display name of the DbContext being migrated.</param>
    /// <param name="cancellationToken">A token that cancels lock acquisition.</param>
    /// <typeparam name="TContext">The module DbContext type being migrated.</typeparam>
    /// <returns>A task that completes once the advisory lock has been acquired.</returns>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken" /> is canceled.</exception>
    /// <exception cref="DbUpdateException">Thrown when PostgreSQL rejects the lock command.</exception>
    private static async Task AcquireMigrationLockAsync<TContext>(
        TContext dbContext,
        ILogger logger,
        string contextName,
        CancellationToken cancellationToken)
        where TContext : DbContext
    {
        logger.LogInformation("Acquiring migration advisory lock for {DbContext}.", contextName);
        await dbContext.Database.ExecuteSqlRawAsync(
            $"SELECT pg_advisory_lock({AdvisoryLockKey})",
            cancellationToken);
        logger.LogInformation("Acquired migration advisory lock for {DbContext}.", contextName);
    }

    /// <summary>
    /// Releases the PostgreSQL advisory lock held by the current database connection.
    /// </summary>
    /// <param name="dbContext">The DbContext whose open connection holds the lock.</param>
    /// <param name="logger">The logger that records lock release progress.</param>
    /// <param name="contextName">The display name of the DbContext being migrated.</param>
    /// <typeparam name="TContext">The module DbContext type being migrated.</typeparam>
    /// <returns>A task that completes after attempting to release the advisory lock.</returns>
    private static async Task ReleaseMigrationLockAsync<TContext>(
        TContext dbContext,
        ILogger logger,
        string contextName)
        where TContext : DbContext
    {
        try
        {
            await dbContext.Database.ExecuteSqlRawAsync($"SELECT pg_advisory_unlock({AdvisoryLockKey})");
            logger.LogInformation("Released migration advisory lock for {DbContext}.", contextName);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Failed to release migration advisory lock for {DbContext}.", contextName);
        }
    }
}
