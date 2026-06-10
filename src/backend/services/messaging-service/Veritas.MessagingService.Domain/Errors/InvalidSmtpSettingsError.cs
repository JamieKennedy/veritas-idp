using Veritas.Shared.Errors;

namespace Veritas.MessagingService.Domain.Errors;

/// <summary>
/// Error returned when SMTP settings fail validation.
/// </summary>
public sealed class InvalidSmtpSettingsError(string message) : ValidationError(ErrorCode, message)
{
    public const string ErrorCode = "MESSAGING_SMTP_SETTINGS_INVALID";
}
