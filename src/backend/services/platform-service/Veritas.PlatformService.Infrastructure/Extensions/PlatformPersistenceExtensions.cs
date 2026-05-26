using Microsoft.Extensions.DependencyInjection;
using Veritas.PlatformService.Application.Persistence;
using Veritas.PlatformService.Infrastructure.Database;

namespace Veritas.PlatformService.Infrastructure.Extensions;

/// <summary>
/// Registers Platform module persistence services.
/// </summary>
public static class PlatformPersistenceExtensions
{
    /// <summary>
    /// Adds the Platform module DbContext and its application persistence port.
    /// </summary>
    /// <param name="services">The service collection receiving Platform persistence services.</param>
    /// <param name="connectionString">The Postgres connection string for the shared Veritas database.</param>
    /// <returns>The same service collection so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services" /> is <see langword="null" />.</exception>
    public static IServiceCollection AddPlatformPersistence(
        this IServiceCollection services,
        string? connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddDbContext<PlatformDbContext>(options =>
            PlatformDbContextOptions.Configure(options, connectionString));
        services.AddScoped<IPlatformDbContext>(provider => provider.GetRequiredService<PlatformDbContext>());

        return services;
    }
}
