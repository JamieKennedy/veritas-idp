using Veritas.MessagingService.Domain.Types;

namespace Veritas.MessagingService.Domain.Entities;

/// <summary>
/// Stores the singleton SMTP configuration for the Veritas installation.
/// </summary>
public sealed class SmtpSettings
{
    /// <summary>
    /// Stable singleton key for the instance SMTP configuration.
    /// </summary>
    public const string SingletonId = "default";

    /// <summary>
    /// The singleton row identifier.
    /// </summary>
    public string Id { get; set; } = SingletonId;

    /// <summary>
    /// SMTP host name or IP address.
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// SMTP TCP port in the range 1-65535.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// TLS behavior used when connecting to the SMTP server.
    /// </summary>
    public SmtpTlsMode TlsMode { get; set; }

    /// <summary>
    /// Optional SMTP username used for authenticated relays.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Data Protection protected SMTP password or secret. Null means no SMTP secret is configured.
    /// </summary>
    public string? ProtectedSecret { get; set; }

    /// <summary>
    /// Email address used as the sender address for Veritas-generated mail.
    /// </summary>
    public string FromEmail { get; set; } = string.Empty;

    /// <summary>
    /// Optional display name used with <see cref="FromEmail" />.
    /// </summary>
    public string? FromName { get; set; }

    /// <summary>
    /// True only after settings have passed a live SMTP test.
    /// </summary>
    public bool IsConfigured { get; set; }

    /// <summary>
    /// UTC instant when SMTP settings last passed a live test send.
    /// </summary>
    public DateTime? LastSuccessfulTestAtUtc { get; set; }

    /// <summary>
    /// UTC instant when this row was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// UTC instant when this row was last updated.
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; }
}
