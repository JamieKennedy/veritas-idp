using Veritas.MessagingService.Domain.Types;

namespace Veritas.MessagingService.Application.DataTransferObjects;

/// <summary>
/// Safe SMTP settings projection that never includes the raw or protected SMTP secret.
/// </summary>
/// <param name="Host">SMTP host name or IP address.</param>
/// <param name="Port">SMTP port.</param>
/// <param name="TlsMode">TLS behavior for SMTP connections.</param>
/// <param name="Username">Optional SMTP username.</param>
/// <param name="HasSecret">Indicates whether a protected SMTP secret is stored.</param>
/// <param name="FromEmail">Sender email address.</param>
/// <param name="FromName">Optional sender display name.</param>
/// <param name="IsConfigured">True when settings have passed a test send.</param>
/// <param name="LastSuccessfulTestAtUtc">UTC instant of the last successful test send.</param>
public sealed record SmtpSettingsDto(
    string Host,
    int Port,
    SmtpTlsMode TlsMode,
    string? Username,
    bool HasSecret,
    string FromEmail,
    string? FromName,
    bool IsConfigured,
    DateTime? LastSuccessfulTestAtUtc);
