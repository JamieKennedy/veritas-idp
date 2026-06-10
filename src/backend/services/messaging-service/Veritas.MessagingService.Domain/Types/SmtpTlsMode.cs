namespace Veritas.MessagingService.Domain.Types;

/// <summary>
/// Describes the TLS behavior used by the SMTP client.
/// </summary>
public enum SmtpTlsMode
{
    /// <summary>
    /// Sends SMTP without TLS. Intended for local development relays such as Mailpit.
    /// </summary>
    None = 0,

    /// <summary>
    /// Requests STARTTLS or equivalent TLS behavior from the SMTP client.
    /// </summary>
    StartTls = 1
}
