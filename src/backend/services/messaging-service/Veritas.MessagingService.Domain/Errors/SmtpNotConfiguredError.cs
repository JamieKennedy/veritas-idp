using Veritas.Shared.Errors;

namespace Veritas.MessagingService.Domain.Errors;

/// <summary>
/// Error returned when email delivery is requested before SMTP is configured.
/// </summary>
public sealed class SmtpNotConfiguredError() : ConflictError(ErrorCode, "SMTP must be configured before email can be sent.")
{
    public const string ErrorCode = "MESSAGING_SMTP_NOT_CONFIGURED";
}
