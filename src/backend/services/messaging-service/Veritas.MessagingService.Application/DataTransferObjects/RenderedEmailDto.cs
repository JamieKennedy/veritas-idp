namespace Veritas.MessagingService.Application.DataTransferObjects;

/// <summary>
/// Rendered email content ready for SMTP delivery.
/// </summary>
/// <param name="Subject">Rendered subject.</param>
/// <param name="HtmlBody">Rendered HTML body.</param>
/// <param name="TextBody">Rendered text body.</param>
public sealed record RenderedEmailDto(string Subject, string HtmlBody, string TextBody);
