using Microsoft.Extensions.DependencyInjection;
using Veritas.UserService.Application.Persistence;
using Veritas.UserService.Infrastructure.Database;

namespace Veritas.UserService.Infrastructure.Extensions;

/// <summary>
/// Registers Users module persistence services.
/// </summary>
public static class UserPersistenceExtensions
{
    /// <summary>
    /// Adds the Users module DbContext and its application persistence port.
    /// </summary>
    /// <param name="services">The service collection receiving Users persistence services.</param>
    /// <param name="connectionString">The Postgres connection string for the shared Veritas database.</param>
    /// <returns>The same service collection so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services" /> is <see langword="null" />.</exception>
    public static IServiceCollection AddUserPersistence(
        this IServiceCollection services,
        string? connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddDbContext<UserDbContext>(options =>
            UserDbContextOptions.Configure(options, connectionString));
        services.AddScoped<IUserDbContext>(provider => provider.GetRequiredService<UserDbContext>());

        return services;
    }
}
