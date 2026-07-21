using Veritas.Shared.Errors;

namespace Veritas.MessagingService.Domain.Errors;

/// <summary>
/// Error returned when a template key cannot be resolved.
/// </summary>
public sealed class TemplateNotFoundError()
    : NotFoundError(ErrorCode, "Email template was not found.")
{
    public const string ErrorCode = "MESSAGING_TEMPLATE_NOT_FOUND";
}
