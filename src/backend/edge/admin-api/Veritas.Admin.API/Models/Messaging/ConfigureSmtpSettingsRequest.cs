using Veritas.MessagingService.Domain.Types;

namespace Veritas.Admin.API.Models.Messaging;

/// <summary>
/// Admin API request for configuring SMTP settings.
/// </summary>
/// <param name="Host">SMTP host name or IP address.</param>
/// <param name="Port">SMTP port in the range 1-65535.</param>
/// <param name="TlsMode">TLS behavior for SMTP connections.</param>
/// <param name="Username">Optional SMTP username.</param>
/// <param name="Secret">Optional raw SMTP secret. This value must not be logged.</param>
/// <param name="FromEmail">Sender email address.</param>
/// <param name="FromName">Optional sender display name.</param>
public sealed record ConfigureSmtpSettingsRequest(
    string Host,
    int Port,
    SmtpTlsMode TlsMode,
    string? Username,
    string? Secret,
    string FromEmail,
    string? FromName);
