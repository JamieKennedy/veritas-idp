using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Veritas.Admin.API.Extensions;

/// <summary>
/// Configures Admin API controllers and their shared HTTP serialization contract.
/// </summary>
public static class AdminControllerExtensions
{
    /// <summary>
    /// Adds Admin API controllers with antiforgery validation and string-based enum serialization.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The MVC builder for additional host configuration.</returns>
    public static IMvcBuilder AddAdminControllers(this IServiceCollection services)
    {
        return services
            .AddControllers(options =>
            {
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
            });
    }
}
