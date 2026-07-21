using Veritas.Shared.Errors;

namespace Veritas.MessagingService.Domain.Errors;

/// <summary>
/// Error returned when template model JSON does not provide required placeholder values.
/// </summary>
public sealed class InvalidTemplateModelError()
    : ValidationError(ErrorCode, "Template model is invalid for the selected template.")
{
    public const string ErrorCode = "MESSAGING_TEMPLATE_MODEL_INVALID";
}
