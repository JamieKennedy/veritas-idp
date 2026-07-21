using Asp.Versioning.ApiExplorer;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;

namespace Veritas.Admin.API.Helpers;

public class ConfigureOpenApiOptions : IConfigureNamedOptions<OpenApiOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureOpenApiOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    public void Configure(string? name, OpenApiOptions options)
    {
        // 'name' here corresponds to the document name (e.g., "v1")
        if (name == null)
        {
            return;
        }

        var description = _provider.ApiVersionDescriptions
            .FirstOrDefault(d => d.GroupName == name);

        if (description == null)
        {
            return;
        }

        // Filter only the endpoints for the specified API version
        options.ShouldInclude = (apiDesc) => apiDesc.GroupName == name;

        // Additional document metadata
        options.AddDocumentTransformer((document, _, _) =>
        {
            document.Info.Title = "Veritas Admin API";
            document.Info.Version = description.ApiVersion.ToString();
            document.Info.Description = description.IsDeprecated
                ? "This version is deprecated."
                : "Modern versioned API.";
            return Task.CompletedTask;
        });
    }

    public void Configure(OpenApiOptions options) => Configure(Options.DefaultName, options);
}
