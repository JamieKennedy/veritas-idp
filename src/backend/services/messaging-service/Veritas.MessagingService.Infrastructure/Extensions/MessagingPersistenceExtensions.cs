using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Veritas.MessagingService.Application.Persistence;
using Veritas.MessagingService.Infrastructure.Database;

namespace Veritas.MessagingService.Infrastructure.Extensions;

/// <summary>
/// Registers Messaging persistence services.
/// </summary>
public static class MessagingPersistenceExtensions
{
    /// <summary>
    /// Adds the Messaging DbContext backed by Postgres.
    /// </summary>
    /// <param name="services">The service collection receiving persistence services.</param>
    /// <param name="connectionString">The Veritas database connection string.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMessagingPersistence(this IServiceCollection services, string? connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddDbContext<MessagingDbContext>(options =>
            options.UseNpgsql(
                connectionString,
                npgsql => npgsql.MigrationsHistoryTable(MessagingDbContextOptions.MigrationsHistoryTable, "messaging")));
        services.AddScoped<IMessagingDbContext>(provider => provider.GetRequiredService<MessagingDbContext>());

        return services;
    }
}
