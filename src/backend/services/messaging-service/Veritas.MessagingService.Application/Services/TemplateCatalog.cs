namespace Veritas.MessagingService.Application.Services;

/// <summary>
/// Central catalog of template keys and their allowed placeholders.
/// </summary>
public static class TemplateCatalog
{
    private static readonly IReadOnlyDictionary<string, IReadOnlySet<string>> Placeholders =
        new Dictionary<string, IReadOnlySet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["admin.welcome"] = new HashSet<string>(StringComparer.Ordinal)
            {
                "User.Email",
                "User.DisplayName",
                "Action.Url"
            },
            ["system.smtp-test"] = new HashSet<string>(StringComparer.Ordinal)
            {
                "System.InstanceName",
                "System.TimestampUtc"
            }
        };

    /// <summary>
    /// Gets the allowlisted placeholders for a template key.
    /// </summary>
    /// <param name="templateKey">The logical template key.</param>
    /// <returns>The allowlisted placeholders for the key, or an empty set when unknown.</returns>
    public static IReadOnlySet<string> GetAllowedPlaceholders(string templateKey)
    {
        return Placeholders.TryGetValue(templateKey, out var placeholders)
            ? placeholders
            : new HashSet<string>(StringComparer.Ordinal);
    }

    /// <summary>
    /// Indicates whether a placeholder is allowed for a template key.
    /// </summary>
    /// <param name="templateKey">The logical template key.</param>
    /// <param name="placeholder">The placeholder path without braces.</param>
    /// <returns>True when the placeholder can be used by the template.</returns>
    public static bool IsAllowed(string templateKey, string placeholder)
    {
        return GetAllowedPlaceholders(templateKey).Contains(placeholder);
    }
}
