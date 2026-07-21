using Veritas.MessagingService.Domain.Types;

namespace Veritas.MessagingService.Application.Services;

/// <summary>
/// Full SMTP delivery request containing configured transport settings and rendered content.
/// </summary>
/// <param name="SmtpHost">SMTP host name or IP address.</param>
/// <param name="SmtpPort">SMTP port.</param>
/// <param name="TlsMode">TLS behavior for SMTP connections.</param>
/// <param name="Username">Optional SMTP username.</param>
/// <param name="Secret">Optional raw SMTP secret. This value must not be logged.</param>
/// <param name="FromEmail">Sender email address.</param>
/// <param name="FromName">Optional sender display name.</param>
/// <param name="ToEmail">Recipient email address.</param>
/// <param name="Subject">Rendered subject.</param>
/// <param name="HtmlBody">Rendered HTML body. This may contain sensitive user data and must not be logged.</param>
/// <param name="TextBody">Rendered text body. This may contain sensitive user data and must not be logged.</param>
public sealed record EmailSendRequest(
    string SmtpHost,
    int SmtpPort,
    SmtpTlsMode TlsMode,
    string? Username,
    string? Secret,
    string FromEmail,
    string? FromName,
    string ToEmail,
    string Subject,
    string HtmlBody,
    string TextBody);
