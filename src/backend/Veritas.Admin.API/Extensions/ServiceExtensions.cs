using System.Text.Json.Nodes;
using Asp.Versioning;
using Microsoft.OpenApi;
using Veritas.Admin.API.Helpers;
using Veritas.Admin.Application.Interfaces;
using Veritas.Admin.Application.Services;
using Veritas.Domain.Repositories;
using Veritas.Infrastructure.Database;
using Veritas.Infrastructure.Database.Repositories;

namespace Veritas.Admin.API.Extensions;

public static class ServiceExtensions
{
    public static void ConfigureSqlContext(this WebApplicationBuilder builder)
    {
        builder.AddNpgsqlDbContext<VeritasDbContext>("VeritasDb");
    }

    public static void ConfigureRepositories(this IServiceCollection services)
    {
        services.AddScoped<ISystemFlagRepository, SystemFlagRepository>();
    }
    
    public static void ConfigureServices(this IServiceCollection services)
    {
        services.AddScoped<ISystemFlagService, SystemFlagService>();
    }
    
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

        // Configure the openAPI doc
        services.ConfigureOptions<ConfigureOpenApiOptions>();

        // Register v1 endpoint, add additional if future versions are created
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

    public static void ConfigureOpenApi(this IServiceCollection services)
    {
        services.AddOpenApi();
    }
    
    
}