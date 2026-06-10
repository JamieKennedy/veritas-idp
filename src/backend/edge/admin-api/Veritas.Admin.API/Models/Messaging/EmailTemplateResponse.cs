namespace Veritas.Admin.API.Models.Messaging;

/// <summary>
/// Admin API response for an editable email template.
/// </summary>
/// <param name="TenantId">Optional tenant identifier. Null means global template.</param>
/// <param name="TemplateKey">Logical template key.</param>
/// <param name="Subject">Subject template.</param>
/// <param name="HtmlBody">HTML body template.</param>
/// <param name="TextBody">Text body template.</param>
/// <param name="IsEnabled">Indicates whether the template can be used for delivery.</param>
/// <param name="UpdatedAtUtc">UTC instant when the template was last updated.</param>
public sealed record EmailTemplateResponse(
    Guid? TenantId,
    string TemplateKey,
    string Subject,
    string HtmlBody,
    string TextBody,
    bool IsEnabled,
    DateTime UpdatedAtUtc);
