using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Veritas.Admin.API.Models.Messaging;
using Veritas.MessagingService.Application.DataTransferObjects;
using Veritas.MessagingService.Application.Services;
using Veritas.MessagingService.Domain.Entities;
using Veritas.Shared.Http;

namespace Veritas.Admin.API.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/messaging")]
[ApiController]
[Authorize]
public sealed class MessagingController(
    ILogger<MessagingController> logger,
    SmtpSettingsService smtpSettingsService,
    TemplateService templateService) : BaseController<MessagingController>(logger)
{
    /// <summary>
    /// Gets safe SMTP settings without exposing secrets.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>The configured SMTP settings.</returns>
    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings(CancellationToken cancellationToken)
    {
        var result = await smtpSettingsService.GetSettingsAsync(cancellationToken);
        return result.IsFailed
            ? FailureResultMapper.ToProblemDetails(result)
            : Ok(ToResponse(result.Value));
    }

    /// <summary>
    /// Validates and stores SMTP settings after a live test send succeeds.
    /// </summary>
    /// <param name="request">The SMTP settings request.</param>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>The safe persisted SMTP settings.</returns>
    [HttpPut("settings/smtp")]
    public async Task<IActionResult> ConfigureSmtp(
        [FromBody] ConfigureSmtpSettingsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await smtpSettingsService.ConfigureSmtpAsync(
            new ConfigureSmtpSettingsDto(
                request.Host,
                request.Port,
                request.TlsMode,
                request.Username,
                request.Secret,
                request.FromEmail,
                request.FromName),
            cancellationToken);

        return result.IsFailed
            ? FailureResultMapper.ToProblemDetails(result)
            : Ok(ToResponse(result.Value));
    }

    /// <summary>
    /// Lists email templates visible to administrators.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>The stored email templates.</returns>
    [HttpGet("templates")]
    public async Task<IActionResult> ListTemplates(CancellationToken cancellationToken)
    {
        var templates = await templateService.ListTemplatesAsync(cancellationToken);
        return Ok(templates.Select(ToResponse));
    }

    /// <summary>
    /// Gets a global email template by key.
    /// </summary>
    /// <param name="templateKey">The template key.</param>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>The matching global email template.</returns>
    [HttpGet("templates/{templateKey}")]
    public async Task<IActionResult> GetTemplate(string templateKey, CancellationToken cancellationToken)
    {
        var result = await templateService.ResolveTemplateAsync(null, templateKey, cancellationToken);
        return result.IsFailed
            ? FailureResultMapper.ToProblemDetails(result)
            : Ok(ToResponse(result.Value));
    }

    /// <summary>
    /// Updates a global email template.
    /// </summary>
    /// <param name="templateKey">The template key.</param>
    /// <param name="request">The template update request.</param>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>The updated email template.</returns>
    [HttpPut("templates/{templateKey}")]
    public async Task<IActionResult> UpdateTemplate(
        string templateKey,
        [FromBody] UpdateEmailTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await templateService.UpdateTemplateAsync(
            templateKey,
            new UpdateEmailTemplateDto(request.Subject, request.HtmlBody, request.TextBody, request.IsEnabled),
            cancellationToken);

        return result.IsFailed
            ? FailureResultMapper.ToProblemDetails(result)
            : Ok(ToResponse(result.Value));
    }

    private static SmtpSettingsResponse ToResponse(SmtpSettingsDto settings)
    {
        return new SmtpSettingsResponse(
            settings.Host,
            settings.Port,
            settings.TlsMode,
            settings.Username,
            settings.HasSecret,
            settings.FromEmail,
            settings.FromName,
            settings.IsConfigured,
            settings.LastSuccessfulTestAtUtc);
    }

    private static EmailTemplateResponse ToResponse(EmailTemplate template)
    {
        return new EmailTemplateResponse(
            template.TenantId,
            template.TemplateKey,
            template.Subject,
            template.HtmlBody,
            template.TextBody,
            template.IsEnabled,
            template.UpdatedAtUtc);
    }
}
