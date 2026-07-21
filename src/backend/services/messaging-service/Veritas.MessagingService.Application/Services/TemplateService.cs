using FluentResults;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Veritas.MessagingService.Application.DataTransferObjects;
using Veritas.MessagingService.Application.Persistence;
using Veritas.MessagingService.Domain.Entities;
using Veritas.MessagingService.Domain.Errors;

namespace Veritas.MessagingService.Application.Services;

/// <summary>
/// Manages email templates and tenant-ready template resolution.
/// </summary>
public sealed class TemplateService(ILogger<TemplateService> logger, IMessagingDbContext dbContext)
{
    /// <summary>
    /// Lists all email templates ordered by key and tenant scope.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the query.</param>
    /// <returns>The email templates currently stored by Messaging.</returns>
    public async Task<IReadOnlyList<EmailTemplate>> ListTemplatesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.EmailTemplates
            .OrderBy(item => item.TemplateKey)
            .ThenBy(item => item.TenantId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Resolves a template by tenant override first, then global fallback.
    /// </summary>
    /// <param name="tenantId">Optional tenant identifier.</param>
    /// <param name="templateKey">The template key to resolve.</param>
    /// <param name="cancellationToken">A token that cancels the query.</param>
    /// <returns>The resolved enabled template.</returns>
    public async Task<Result<EmailTemplate>> ResolveTemplateAsync(
        Guid? tenantId,
        string templateKey,
        CancellationToken cancellationToken = default)
    {
        var normalizedKey = templateKey.Trim();
        var template = tenantId is null
            ? null
            : await dbContext.EmailTemplates
                .Where(item => item.TenantId == tenantId && item.TemplateKey == normalizedKey && item.IsEnabled)
                .SingleOrDefaultAsync(cancellationToken);

        template ??= await dbContext.EmailTemplates
            .Where(item => item.TenantId == null && item.TemplateKey == normalizedKey && item.IsEnabled)
            .SingleOrDefaultAsync(cancellationToken);

        return template is null
            ? Result.Fail(new TemplateNotFoundError())
            : Result.Ok(template);
    }

    /// <summary>
    /// Updates a global template after validating placeholder allowlists.
    /// </summary>
    /// <param name="templateKey">The global template key.</param>
    /// <param name="request">The editable template content.</param>
    /// <param name="cancellationToken">A token that cancels the update.</param>
    /// <returns>The updated template.</returns>
    public async Task<Result<EmailTemplate>> UpdateTemplateAsync(
        string templateKey,
        UpdateEmailTemplateDto request,
        CancellationToken cancellationToken = default)
    {
        var normalizedKey = templateKey.Trim();
        var placeholders = TemplatePlaceholderParser.Extract(request.Subject, request.HtmlBody, request.TextBody);
        foreach (var placeholder in placeholders)
        {
            if (!TemplateCatalog.IsAllowed(normalizedKey, placeholder))
            {
                return Result.Fail(new UnknownTemplatePlaceholderError(placeholder));
            }
        }

        var template = await dbContext.EmailTemplates
            .SingleOrDefaultAsync(
                item => item.TenantId == null && item.TemplateKey == normalizedKey,
                cancellationToken);

        if (template is null)
        {
            return Result.Fail(new TemplateNotFoundError());
        }

        template.Subject = request.Subject;
        template.HtmlBody = request.HtmlBody;
        template.TextBody = request.TextBody;
        template.IsEnabled = request.IsEnabled;
        template.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Updated email template {TemplateKey}.", normalizedKey);
        }
        return Result.Ok(template);
    }
}
