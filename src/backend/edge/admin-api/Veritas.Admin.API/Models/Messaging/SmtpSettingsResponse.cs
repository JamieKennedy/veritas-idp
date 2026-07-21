using Veritas.MessagingService.Domain.Types;

namespace Veritas.Admin.API.Models.Messaging;

/// <summary>
/// Safe SMTP settings response that never contains SMTP secrets.
/// </summary>
/// <param name="Host">SMTP host name or IP address.</param>
/// <param name="Port">SMTP port.</param>
/// <param name="TlsMode">TLS behavior for SMTP connections.</param>
/// <param name="Username">Optional SMTP username.</param>
/// <param name="HasSecret">Indicates whether an SMTP secret is stored.</param>
/// <param name="FromEmail">Sender email address.</param>
/// <param name="FromName">Optional sender display name.</param>
/// <param name="IsConfigured">True when settings have passed a test send.</param>
/// <param name="LastSuccessfulTestAtUtc">UTC instant of the last successful test send.</param>
public sealed record SmtpSettingsResponse(
    string Host,
    int Port,
    SmtpTlsMode TlsMode,
    string? Username,
    bool HasSecret,
    string FromEmail,
    string? FromName,
    bool IsConfigured,
    DateTime? LastSuccessfulTestAtUtc);
