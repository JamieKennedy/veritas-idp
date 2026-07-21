namespace Veritas.MessagingService.Application.DataTransferObjects;

/// <summary>
/// Request DTO used to update editable template content.
/// </summary>
/// <param name="Subject">Subject template.</param>
/// <param name="HtmlBody">HTML body template.</param>
/// <param name="TextBody">Text body template.</param>
/// <param name="IsEnabled">Indicates whether the template can be used for delivery.</param>
public sealed record UpdateEmailTemplateDto(
    string Subject,
    string HtmlBody,
    string TextBody,
    bool IsEnabled);
