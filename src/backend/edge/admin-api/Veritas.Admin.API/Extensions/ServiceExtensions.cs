using System.Text.Json.Nodes;
using Asp.Versioning;
using FluentResults;
using Microsoft.OpenApi;
using Veritas.Admin.API.Helpers;
using Veritas.PlatformService.Application.Dependencies;
using Veritas.PlatformService.Application.Interfaces;
using Veritas.PlatformService.Application.Services;
using Veritas.PlatformService.Infrastructure.Extensions;
using Veritas.UserService.Application.Interfaces;
using Veritas.UserService.Application.Services;
using Veritas.UserService.Infrastructure.Extensions;

namespace Veritas.Admin.API.Extensions;

public static class ServiceExtensions
{
    /// <summary>
    /// Registers Veritas modules and their in-process module adapters.
    /// </summary>
    /// <param name="services">The service collection receiving module services.</param>
    /// <param name="configuration">Application configuration containing module connection strings.</param>
    public static void AddVeritasModules(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("VeritasDb");
        services.AddPlatformPersistence(connectionString);
        services.AddUserPersistence(connectionString);

        services.AddScoped<ISystemFlagService, SystemFlagService>();
        services.AddScoped<IBootstrapService, BootstrapService>();
        services.AddSingleton<IBootstrapSecretValidator>(_ =>
            new ConfiguredBootstrapSecretValidator(ResolveBootstrapSecret(configuration)));
        services.AddScoped<IAdminUserService, AdminUserService>();
        services.AddScoped<IAdminUserDirectory, InProcessAdminUserDirectory>();
        services.AddScoped<IInitialAdminCreator, InProcessInitialAdminCreator>();
    }

    /// <summary>
    /// Configures API versioning and versioned OpenAPI discovery.
    /// </summary>
    /// <param name="services">The service collection receiving API versioning services.</param>
    public static void ConfigureVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(
                new QueryStringApiVersionReader("api-version"),
                new HeaderApiVersionReader("X-Api-Version"),
                new UrlSegmentApiVersionReader()
            );
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        services.ConfigureOptions<ConfigureOpenApiOptions>();

        services.AddOpenApi("v1", options =>
        {
            options.AddSchemaTransformer((schema, context, _) =>
            {
                if (!context.JsonTypeInfo.Type.IsEnum) return Task.CompletedTask;

                schema.Type = JsonSchemaType.String;
                schema.Enum = Enum.GetNames(context.JsonTypeInfo.Type)
                    .Select(name => JsonValue.Create(name))
                    .Cast<JsonNode>()
                    .ToList();
                return Task.CompletedTask;
            });
        });
    }

    /// <summary>
    /// Registers default OpenAPI services.
    /// </summary>
    /// <param name="services">The service collection receiving OpenAPI services.</param>
    public static void ConfigureOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi();
    }

    private sealed class InProcessAdminUserDirectory(IAdminUserService adminUserService) : IAdminUserDirectory
    {
        /// <inheritdoc />
        public async Task<Result<bool>> HasAnyAdminUserAsync(CancellationToken cancellationToken = default)
        {
            var count = await adminUserService.GetAdminUserCountAsync(cancellationToken);
            return count.IsSuccess
                ? Result.Ok(count.Value > 0)
                : count.ToResult<bool>();
        }
    }

    private sealed class InProcessInitialAdminCreator(IAdminUserService adminUserService) : IInitialAdminCreator
    {
        /// <inheritdoc />
        public Task<Result> CreateInitialAdminUserAsync(
            string email,
            string password,
            string? displayName,
            CancellationToken cancellationToken = default)
        {
            return adminUserService.CreateInitialAdminUserAsync(email, password, displayName, cancellationToken);
        }
    }

    /// <summary>
    /// Resolves the deployment bootstrap secret from a file path first, then an environment/config value.
    /// </summary>
    /// <param name="configuration">The configuration source containing bootstrap secret settings.</param>
    /// <returns>The configured bootstrap secret.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no bootstrap secret source is configured.</exception>
    private static string ResolveBootstrapSecret(IConfiguration configuration)
    {
        var secretFile = configuration["BOOTSTRAP_SECRET_FILE"];

        if (!string.IsNullOrWhiteSpace(secretFile))
        {
            var fileSecret = File.ReadAllText(secretFile).Trim();

            if (string.IsNullOrWhiteSpace(fileSecret))
            {
                throw new InvalidOperationException("BOOTSTRAP_SECRET_FILE must point to a non-empty secret file.");
            }

            return fileSecret;
        }

        var secret = configuration["BOOTSTRAP_SECRET"];

        if (!string.IsNullOrWhiteSpace(secret))
        {
            return secret.Trim();
        }

        throw new InvalidOperationException("BOOTSTRAP_SECRET_FILE or BOOTSTRAP_SECRET must be configured.");
    }
}
