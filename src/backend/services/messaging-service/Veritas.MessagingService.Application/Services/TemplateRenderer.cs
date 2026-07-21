using System.Text.Json;

using FluentResults;

using Veritas.MessagingService.Application.DataTransferObjects;
using Veritas.MessagingService.Domain.Entities;
using Veritas.MessagingService.Domain.Errors;

namespace Veritas.MessagingService.Application.Services;

/// <summary>
/// Renders email templates using allowlisted placeholder replacement.
/// </summary>
public sealed class TemplateRenderer
{
    /// <summary>
    /// Renders subject, HTML body, and text body using values from JSON object model.
    /// </summary>
    /// <param name="template">The template to render.</param>
    /// <param name="model">The non-secret JSON model.</param>
    /// <returns>The rendered email content, or a controlled validation failure.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1822:Mark members as static",
        Justification = "TemplateRenderer is an injected application service and intentionally exposes an instance contract.")]
    public Result<RenderedEmailDto> Render(EmailTemplate template, JsonElement model)
    {
        if (model.ValueKind != JsonValueKind.Object)
        {
            return Result.Fail(new InvalidTemplateModelError());
        }

        var placeholders = TemplatePlaceholderParser.Extract(template.Subject, template.HtmlBody, template.TextBody);
        var values = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var placeholder in placeholders)
        {
            if (!TemplateCatalog.IsAllowed(template.TemplateKey, placeholder) ||
                !TryResolveString(model, placeholder, out var value))
            {
                return Result.Fail(new InvalidTemplateModelError());
            }

            values[placeholder] = value;
        }

        return Result.Ok(new RenderedEmailDto(
            Replace(template.Subject, values),
            Replace(template.HtmlBody, values),
            Replace(template.TextBody, values)));
    }

    private static bool TryResolveString(JsonElement model, string path, out string value)
    {
        var current = model;
        foreach (var segment in path.Split('.', StringSplitOptions.RemoveEmptyEntries))
        {
            if (current.ValueKind != JsonValueKind.Object ||
                !current.TryGetProperty(segment, out current))
            {
                value = string.Empty;
                return false;
            }
        }

        if (current.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            value = string.Empty;
            return false;
        }

        value = current.ValueKind == JsonValueKind.String
            ? current.GetString() ?? string.Empty
            : current.ToString();
        return !string.IsNullOrWhiteSpace(value);
    }

    private static string Replace(string templatePart, IReadOnlyDictionary<string, string> values)
    {
        var rendered = templatePart;
        foreach (var pair in values)
        {
            rendered = rendered.Replace("{{" + pair.Key + "}}", pair.Value, StringComparison.Ordinal)
                .Replace("{{ " + pair.Key + " }}", pair.Value, StringComparison.Ordinal);
        }

        return rendered;
    }
}
