namespace Veritas.Admin.API.Models.Messaging;

/// <summary>
/// Admin API request for updating global template content.
/// </summary>
/// <param name="Subject">Subject template.</param>
/// <param name="HtmlBody">HTML body template.</param>
/// <param name="TextBody">Text body template.</param>
/// <param name="IsEnabled">Indicates whether the template can be used for delivery.</param>
public sealed record UpdateEmailTemplateRequest(
    string Subject,
    string HtmlBody,
    string TextBody,
    bool IsEnabled);
