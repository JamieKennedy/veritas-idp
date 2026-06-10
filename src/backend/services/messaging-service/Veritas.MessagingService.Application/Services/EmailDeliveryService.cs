using System.Net.Mail;
using System.Text.Json;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Veritas.MessagingService.Application.Persistence;
using Veritas.MessagingService.Domain.Entities;
using Veritas.MessagingService.Domain.Errors;
using Veritas.Shared.Security;

namespace Veritas.MessagingService.Application.Services;

/// <summary>
/// Resolves SMTP settings and templates, renders content, and delegates delivery to the configured adapter.
/// </summary>
public sealed class EmailDeliveryService(
    ILogger<EmailDeliveryService> logger,
    IMessagingDbContext dbContext,
    ISecretProtector secretProtector,
    TemplateRenderer templateRenderer,
    IEmailSender emailSender)
{
    /// <summary>
    /// Sends a templated email using a non-secret JSON model.
    /// </summary>
    /// <param name="tenantId">Optional tenant identifier used for template overrides.</param>
    /// <param name="toEmail">Recipient email address.</param>
    /// <param name="templateKey">The template key to render.</param>
    /// <param name="templateModelJson">A JSON object containing non-secret template values.</param>
    /// <param name="cancellationToken">A token that cancels delivery.</param>
    /// <returns>A success result when the email is accepted by the adapter.</returns>
    public async Task<Result> SendTemplatedEmailAsync(
        Guid? tenantId,
        string toEmail,
        string templateKey,
        string templateModelJson,
        CancellationToken cancellationToken = default)
    {
        if (!IsValidEmail(toEmail))
        {
            return Result.Fail(new InvalidTemplateModelError());
        }

        var smtpSettings = await dbContext.SmtpSettings
            .SingleOrDefaultAsync(
                item => item.Id == SmtpSettings.SingletonId && item.IsConfigured,
                cancellationToken);

        if (smtpSettings is null)
        {
            return Result.Fail(new SmtpNotConfiguredError());
        }

        var template = await ResolveTemplateAsync(tenantId, templateKey.Trim(), cancellationToken);
        if (template is null)
        {
            return Result.Fail(new TemplateNotFoundError());
        }

        JsonDocument model;
        try
        {
            model = JsonDocument.Parse(templateModelJson);
        }
        catch (JsonException)
        {
            return Result.Fail(new InvalidTemplateModelError());
        }

        using (model)
        {
            var rendered = templateRenderer.Render(template, model.RootElement);
            if (rendered.IsFailed)
            {
                return rendered.ToResult();
            }

            var sendResult = await emailSender.SendAsync(
                new EmailSendRequest(
                    smtpSettings.Host,
                    smtpSettings.Port,
                    smtpSettings.TlsMode,
                    smtpSettings.Username,
                    string.IsNullOrWhiteSpace(smtpSettings.ProtectedSecret)
                        ? null
                        : secretProtector.Unprotect(smtpSettings.ProtectedSecret),
                    smtpSettings.FromEmail,
                    smtpSettings.FromName,
                    toEmail.Trim().ToLowerInvariant(),
                    rendered.Value.Subject,
                    rendered.Value.HtmlBody,
                    rendered.Value.TextBody),
                cancellationToken);

            if (sendResult.IsFailed)
            {
                logger.LogWarning(
                    "Templated email delivery failed for template {TemplateKey}.",
                    templateKey);
                return sendResult;
            }
        }

        logger.LogInformation("Templated email delivery accepted for template {TemplateKey}.", templateKey);
        return Result.Ok();
    }

    private async Task<EmailTemplate?> ResolveTemplateAsync(
        Guid? tenantId,
        string templateKey,
        CancellationToken cancellationToken)
    {
        var template = tenantId is null
            ? null
            : await dbContext.EmailTemplates
                .Where(item => item.TenantId == tenantId && item.TemplateKey == templateKey && item.IsEnabled)
                .SingleOrDefaultAsync(cancellationToken);

        return template ?? await dbContext.EmailTemplates
            .Where(item => item.TenantId == null && item.TemplateKey == templateKey && item.IsEnabled)
            .SingleOrDefaultAsync(cancellationToken);
    }

    private static bool IsValidEmail(string value)
    {
        try
        {
            _ = new MailAddress(value);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
