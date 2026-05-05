using System.Text.Json.Nodes;
using Asp.Versioning;
using Grpc.Net.ClientFactory;
using Microsoft.OpenApi;
using Veritas.Admin.API.Gateways;
using Veritas.Admin.API.Helpers;
using Veritas.PlatformService.Contracts.Grpc;

namespace Veritas.Admin.API.Extensions;

public static class ServiceExtensions
{
    public static void ConfigurePlatformGrpcClients(this IServiceCollection services)
    {
        services.AddGrpcClient<PlatformSetup.PlatformSetupClient>(options =>
        {
            options.Address = new Uri("https+http://platform-service-api");
        });
        services.AddScoped<PlatformSetupGateway>();
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
