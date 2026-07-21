using Veritas.MessagingService.Domain.Types;

namespace Veritas.MessagingService.Application.Services;

/// <summary>
/// SMTP connection settings used for a live test send.
/// </summary>
/// <param name="Host">SMTP host name or IP address.</param>
/// <param name="Port">SMTP port.</param>
/// <param name="TlsMode">TLS behavior for SMTP connections.</param>
/// <param name="Username">Optional SMTP username.</param>
/// <param name="Secret">Optional raw SMTP secret. This value must not be logged.</param>
/// <param name="FromEmail">Sender email address.</param>
/// <param name="FromName">Optional sender display name.</param>
public sealed record SmtpConnectivityTestRequest(
    string Host,
    int Port,
    SmtpTlsMode TlsMode,
    string? Username,
    string? Secret,
    string FromEmail,
    string? FromName);
