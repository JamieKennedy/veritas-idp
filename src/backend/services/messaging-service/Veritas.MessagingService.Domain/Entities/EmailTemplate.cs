namespace Veritas.MessagingService.Domain.Entities;

/// <summary>
/// Stores a tenant-ready email template body and metadata.
/// </summary>
public sealed class EmailTemplate
{
    /// <summary>
    /// Unique template row identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Optional tenant identifier for future tenant-specific overrides. Null means global default.
    /// </summary>
    public Guid? TenantId { get; set; }

    /// <summary>
    /// Logical template key, for example admin.welcome.
    /// </summary>
    public string TemplateKey { get; set; } = string.Empty;

    /// <summary>
    /// Template subject containing only allowlisted placeholders.
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// HTML body containing only allowlisted placeholders.
    /// </summary>
    public string HtmlBody { get; set; } = string.Empty;

    /// <summary>
    /// Plain text body containing only allowlisted placeholders.
    /// </summary>
    public string TextBody { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the template can be used for delivery.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// UTC instant when this template was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// UTC instant when this template was last updated.
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; }
}
