using System.Net.Mail;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Veritas.MessagingService.Application.DataTransferObjects;
using Veritas.MessagingService.Application.Persistence;
using Veritas.MessagingService.Domain.Entities;
using Veritas.MessagingService.Domain.Errors;
using Veritas.MessagingService.Domain.Types;
using Veritas.Shared.Security;

namespace Veritas.MessagingService.Application.Services;

/// <summary>
/// Validates, tests, and stores SMTP settings for the installation.
/// </summary>
public sealed class SmtpSettingsService(
    ILogger<SmtpSettingsService> logger,
    IMessagingDbContext dbContext,
    ISecretProtector secretProtector,
    ISmtpConnectivityTester smtpConnectivityTester) : ISmtpSetupStatus
{
    /// <summary>
    /// Returns the current safe SMTP settings projection.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the query.</param>
    /// <returns>The SMTP settings projection, or a not-configured failure.</returns>
    public async Task<Result<SmtpSettingsDto>> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        var settings = await dbContext.SmtpSettings.SingleOrDefaultAsync(
            item => item.Id == SmtpSettings.SingletonId,
            cancellationToken);

        return settings is null
            ? Result.Fail(new SmtpNotConfiguredError())
            : Result.Ok(ToDto(settings));
    }

    /// <summary>
    /// Indicates whether SMTP has been successfully configured.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the query.</param>
    /// <returns>True when SMTP settings exist and have passed a test send.</returns>
    public Task<bool> IsSmtpConfiguredAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.SmtpSettings.AnyAsync(
            item => item.Id == SmtpSettings.SingletonId && item.IsConfigured,
            cancellationToken);
    }

    /// <summary>
    /// Validates SMTP settings, performs a live test send, and persists the protected settings on success.
    /// </summary>
    /// <param name="request">The raw SMTP settings request.</param>
    /// <param name="cancellationToken">A token that cancels validation, testing, or persistence.</param>
    /// <returns>The persisted safe SMTP settings projection.</returns>
    public async Task<Result<SmtpSettingsDto>> ConfigureSmtpAsync(
        ConfigureSmtpSettingsDto request,
        CancellationToken cancellationToken = default)
    {
        var validation = Validate(request);
        if (validation.IsFailed)
        {
            return validation.ToResult<SmtpSettingsDto>();
        }

        var normalized = Normalize(request);
        var testResult = await smtpConnectivityTester.TestAsync(
            new SmtpConnectivityTestRequest(
                normalized.Host,
                normalized.Port,
                normalized.TlsMode,
                normalized.Username,
                normalized.Secret,
                normalized.FromEmail,
                normalized.FromName),
            cancellationToken);

        if (testResult.IsFailed)
        {
            logger.LogWarning(
                "SMTP settings test failed for host {SmtpHost} and port {SmtpPort}.",
                normalized.Host,
                normalized.Port);
            return testResult.ToResult<SmtpSettingsDto>();
        }

        var now = DateTime.UtcNow;
        var settings = await dbContext.SmtpSettings.SingleOrDefaultAsync(
            item => item.Id == SmtpSettings.SingletonId,
            cancellationToken);

        if (settings is null)
        {
            settings = new SmtpSettings
            {
                Id = SmtpSettings.SingletonId,
                CreatedAtUtc = now
            };
            dbContext.SmtpSettings.Add(settings);
        }

        settings.Host = normalized.Host;
        settings.Port = normalized.Port;
        settings.TlsMode = normalized.TlsMode;
        settings.Username = normalized.Username;
        settings.ProtectedSecret = string.IsNullOrWhiteSpace(normalized.Secret)
            ? null
            : secretProtector.Protect(normalized.Secret);
        settings.FromEmail = normalized.FromEmail;
        settings.FromName = normalized.FromName;
        settings.IsConfigured = true;
        settings.LastSuccessfulTestAtUtc = now;
        settings.UpdatedAtUtc = now;

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("SMTP settings configured for host {SmtpHost} and port {SmtpPort}.", settings.Host, settings.Port);
        return Result.Ok(ToDto(settings));
    }

    private static Result Validate(ConfigureSmtpSettingsDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Host))
        {
            return Result.Fail(new InvalidSmtpSettingsError("SMTP host is required."));
        }

        if (request.Port is < 1 or > 65535)
        {
            return Result.Fail(new InvalidSmtpSettingsError("SMTP port must be between 1 and 65535."));
        }

        if (!Enum.IsDefined(typeof(SmtpTlsMode), request.TlsMode))
        {
            return Result.Fail(new InvalidSmtpSettingsError("SMTP TLS mode is invalid."));
        }

        try
        {
            _ = new MailAddress(request.FromEmail);
        }
        catch (FormatException)
        {
            return Result.Fail(new InvalidSmtpSettingsError("SMTP from email is invalid."));
        }

        return Result.Ok();
    }

    private static ConfigureSmtpSettingsDto Normalize(ConfigureSmtpSettingsDto request)
    {
        return request with
        {
            Host = request.Host.Trim(),
            Username = string.IsNullOrWhiteSpace(request.Username) ? null : request.Username.Trim(),
            Secret = string.IsNullOrWhiteSpace(request.Secret) ? null : request.Secret,
            FromEmail = request.FromEmail.Trim().ToLowerInvariant(),
            FromName = string.IsNullOrWhiteSpace(request.FromName) ? null : request.FromName.Trim()
        };
    }

    private static SmtpSettingsDto ToDto(SmtpSettings settings)
    {
        return new SmtpSettingsDto(
            settings.Host,
            settings.Port,
            settings.TlsMode,
            settings.Username,
            !string.IsNullOrWhiteSpace(settings.ProtectedSecret),
            settings.FromEmail,
            settings.FromName,
            settings.IsConfigured,
            settings.LastSuccessfulTestAtUtc);
    }
}
