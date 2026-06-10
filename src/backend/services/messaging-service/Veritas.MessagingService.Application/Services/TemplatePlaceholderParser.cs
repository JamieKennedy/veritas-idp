using System.Text.RegularExpressions;

namespace Veritas.MessagingService.Application.Services;

/// <summary>
/// Extracts placeholder paths from template text.
/// </summary>
public static partial class TemplatePlaceholderParser
{
    /// <summary>
    /// Extracts unique placeholder paths from the supplied template fragments.
    /// </summary>
    /// <param name="parts">Template fragments to scan.</param>
    /// <returns>Unique placeholder paths without braces.</returns>
    public static IReadOnlySet<string> Extract(params string[] parts)
    {
        var placeholders = new HashSet<string>(StringComparer.Ordinal);

        foreach (var part in parts)
        {
            foreach (Match match in PlaceholderRegex().Matches(part ?? string.Empty))
            {
                placeholders.Add(match.Groups["path"].Value);
            }
        }

        return placeholders;
    }

    [GeneratedRegex("\\{\\{\\s*(?<path>[A-Za-z0-9_.]+)\\s*\\}\\}", RegexOptions.CultureInvariant)]
    private static partial Regex PlaceholderRegex();
}
