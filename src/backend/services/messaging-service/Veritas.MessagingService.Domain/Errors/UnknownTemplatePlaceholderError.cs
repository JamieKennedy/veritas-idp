using Veritas.Shared.Errors;

namespace Veritas.MessagingService.Domain.Errors;

/// <summary>
/// Error returned when a template uses a placeholder outside the template allowlist.
/// </summary>
public sealed class UnknownTemplatePlaceholderError(string placeholder)
    : ValidationError(ErrorCode, $"Template placeholder '{placeholder}' is not allowed.")
{
    public const string ErrorCode = "MESSAGING_TEMPLATE_PLACEHOLDER_UNKNOWN";
}
