using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Serilog;

using Veritas.MessagingService.Infrastructure.Database;
using Veritas.MessagingService.Infrastructure.Extensions;
using Veritas.PlatformService.Infrastructure.Database;
using Veritas.PlatformService.Infrastructure.Extensions;
using Veritas.Tooling.DbMigrator;
using Veritas.UserService.Infrastructure.Database;
using Veritas.UserService.Infrastructure.Extensions;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();

var connectionString = builder.Configuration.GetConnectionString("VeritasDb");
if (string.IsNullOrWhiteSpace(connectionString))
{
    Console.Error.WriteLine("Connection string 'VeritasDb' is required to run database migrations.");
    await Log.CloseAndFlushAsync();
    return 1;
}

builder.Services.AddPlatformPersistence(connectionString);
builder.Services.AddUserPersistence(connectionString);
builder.Services.AddMessagingPersistence(connectionString);
builder.Services.Configure<MigrationOptions>(builder.Configuration.GetSection(MigrationOptions.SectionName));

using var host = builder.Build();
var logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Veritas.Tooling.DbMigrator");

using var cancellationTokenSource = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    logger.LogWarning("Migration cancellation requested from console.");
    cancellationTokenSource.Cancel();
};

try
{
    logger.LogInformation("Starting database migrations.");

    var migrations = new MigrationStep[]
    {
        new("Platform", services => Utilities.MigrateAsync<PlatformDbContext>(services, cancellationTokenSource.Token)),
        new("Users", services => Utilities.MigrateAsync<UserDbContext>(services, cancellationTokenSource.Token)),
        new("Messaging", services => Utilities.MigrateAsync<MessagingDbContext>(services, cancellationTokenSource.Token))
    };

    foreach (var migration in migrations)
    {
        cancellationTokenSource.Token.ThrowIfCancellationRequested();
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Starting {ModuleName} module migrations.", migration.ModuleName);
        }
        await migration.RunAsync(host.Services);
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Completed {ModuleName} module migrations.", migration.ModuleName);
        }
    }

    logger.LogInformation("Database migrations completed.");
    return 0;
}
catch (OperationCanceledException exception)
{
    logger.LogWarning(exception, "Database migration was canceled.");
    return 2;
}
catch (Exception exception)
{
    logger.LogCritical(exception, "Database migration failed.");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}

internal sealed record MigrationStep(
    string ModuleName,
    Func<IServiceProvider, Task> RunAsync);
