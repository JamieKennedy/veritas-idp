namespace Veritas.Admin.API.Extensions;

/// <summary>
/// Registers CORS policies used by the Admin API host.
/// </summary>
public static class AdminCorsExtensions
{
    /// <summary>
    /// Names the CORS policy used by the local Aspire-hosted Admin UI.
    /// </summary>
    public const string AdminUiPolicyName = "AdminUiCors";

    /// <summary>
    /// Adds the Admin UI CORS policy from the configured allowed origins.
    /// </summary>
    /// <param name="services">The service collection that receives CORS services.</param>
    /// <param name="configuration">The application configuration containing <c>Cors:AllowedOrigins</c>.</param>
    /// <returns>The same service collection for additional registrations.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="configuration"/> is <see langword="null" />.</exception>
    public static IServiceCollection AddAdminUiCorsPolicy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var allowedOrigins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy(AdminUiPolicyName, policy =>
            {
                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }
}
