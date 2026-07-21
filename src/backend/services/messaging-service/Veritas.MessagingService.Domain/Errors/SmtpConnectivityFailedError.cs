using Veritas.Shared.Errors;

namespace Veritas.MessagingService.Domain.Errors;

/// <summary>
/// Error returned when the configured SMTP server cannot be reached or rejects a test send.
/// </summary>
public sealed class SmtpConnectivityFailedError()
    : UnexpectedError(ErrorCode, "SMTP connectivity test failed.")
{
    public const string ErrorCode = "MESSAGING_SMTP_CONNECTIVITY_FAILED";
}
