using FluentResults;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

using Veritas.MessagingService.Application.Services;
using Veritas.MessagingService.Domain.Entities;
using Veritas.MessagingService.Domain.Errors;
using Veritas.MessagingService.Domain.Types;
using Veritas.MessagingService.Infrastructure.Database;
using Veritas.Shared.Security;

using Xunit;

namespace Veritas.MessagingService.Application.Tests;

public sealed class EmailDeliveryServiceTests
{
    [Fact]
    public async Task SendTemplatedEmailAsync_renders_and_sends_with_configured_smtp()
    {
        await using var context = CreateContext();
        context.SmtpSettings.Add(ConfiguredSmtpSettings());
        context.EmailTemplates.Add(Template("admin.welcome"));
        await context.SaveChangesAsync();
        var sender = new RecordingEmailSender(Result.Ok());
        var service = CreateService(context, sender);

        var result = await service.SendTemplatedEmailAsync(
            null,
            "admin@example.com",
            "admin.welcome",
            """{"User":{"Email":"admin@example.com","DisplayName":"First Admin"},"Action":{"Url":"https://veritas.example/admin"}}""",
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(sender.LastRequest);
        Assert.Equal("admin@example.com", sender.LastRequest.ToEmail);
        Assert.Equal("Welcome admin@example.com", sender.LastRequest.Subject);
        Assert.Contains("First Admin", sender.LastRequest.HtmlBody, StringComparison.Ordinal);
        Assert.Equal("smtp.example.com", sender.LastRequest.SmtpHost);
        Assert.Equal("super-secret", sender.LastRequest.Secret);
    }

    [Fact]
    public async Task SendTemplatedEmailAsync_fails_safely_when_smtp_is_missing()
    {
        await using var context = CreateContext();
        context.EmailTemplates.Add(Template("admin.welcome"));
        await context.SaveChangesAsync();
        var service = CreateService(context, new RecordingEmailSender(Result.Ok()));

        var result = await service.SendTemplatedEmailAsync(
            null,
            "admin@example.com",
            "admin.welcome",
            """{"User":{"Email":"admin@example.com","DisplayName":"First Admin"},"Action":{"Url":"https://veritas.example/admin"}}""",
            CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.IsType<SmtpNotConfiguredError>(Assert.Single(result.Errors));
    }

    [Fact]
    public async Task SendTemplatedEmailAsync_fails_safely_when_template_is_missing()
    {
        await using var context = CreateContext();
        context.SmtpSettings.Add(ConfiguredSmtpSettings());
        await context.SaveChangesAsync();
        var service = CreateService(context, new RecordingEmailSender(Result.Ok()));

        var result = await service.SendTemplatedEmailAsync(
            null,
            "admin@example.com",
            "missing.template",
            """{"User":{"Email":"admin@example.com"}}""",
            CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.IsType<TemplateNotFoundError>(Assert.Single(result.Errors));
    }

    [Fact]
    public async Task SendTemplatedEmailAsync_fails_safely_when_template_model_is_invalid()
    {
        await using var context = CreateContext();
        context.SmtpSettings.Add(ConfiguredSmtpSettings());
        context.EmailTemplates.Add(Template("admin.welcome"));
        await context.SaveChangesAsync();
        var service = CreateService(context, new RecordingEmailSender(Result.Ok()));

        var result = await service.SendTemplatedEmailAsync(
            null,
            "admin@example.com",
            "admin.welcome",
            """{"User":{"Email":"admin@example.com"}}""",
            CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.IsType<InvalidTemplateModelError>(Assert.Single(result.Errors));
    }

    [Fact]
    public async Task SendTemplatedEmailAsync_returns_controlled_failure_when_smtp_send_fails()
    {
        await using var context = CreateContext();
        context.SmtpSettings.Add(ConfiguredSmtpSettings());
        context.EmailTemplates.Add(Template("admin.welcome"));
        await context.SaveChangesAsync();
        var service = CreateService(context, new RecordingEmailSender(Result.Fail(new SmtpConnectivityFailedError())));

        var result = await service.SendTemplatedEmailAsync(
            null,
            "admin@example.com",
            "admin.welcome",
            """{"User":{"Email":"admin@example.com","DisplayName":"First Admin"},"Action":{"Url":"https://veritas.example/admin"}}""",
            CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.IsType<SmtpConnectivityFailedError>(Assert.Single(result.Errors));
    }

    private static EmailDeliveryService CreateService(MessagingDbContext context, IEmailSender sender)
    {
        return new EmailDeliveryService(
            NullLogger<EmailDeliveryService>.Instance,
            context,
            new RecordingSecretProtector(),
            new TemplateRenderer(),
            sender);
    }

    private static SmtpSettings ConfiguredSmtpSettings()
    {
        return new SmtpSettings
        {
            Id = SmtpSettings.SingletonId,
            Host = "smtp.example.com",
            Port = 1025,
            TlsMode = SmtpTlsMode.None,
            Username = "smtp-user",
            ProtectedSecret = "protected:super-secret",
            FromEmail = "noreply@example.com",
            FromName = "Veritas",
            IsConfigured = true,
            LastSuccessfulTestAtUtc = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
    }

    private static EmailTemplate Template(string key)
    {
        return new EmailTemplate
        {
            Id = Guid.NewGuid(),
            TemplateKey = key,
            Subject = "Welcome {{User.Email}}",
            HtmlBody = "<p>Hello {{User.DisplayName}}</p><p>{{Action.Url}}</p>",
            TextBody = "Hello {{User.DisplayName}} {{Action.Url}}",
            IsEnabled = true,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
    }

    private static MessagingDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MessagingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new MessagingDbContext(options);
    }

    private sealed class RecordingSecretProtector : ISecretProtector
    {
        public string Protect(string plaintext)
        {
            return $"protected:{plaintext}";
        }

        public string Unprotect(string protectedText)
        {
            return protectedText["protected:".Length..];
        }
    }

    private sealed class RecordingEmailSender(Result result) : IEmailSender
    {
        public EmailSendRequest? LastRequest
        {
            get; private set;
        }

        public Task<Result> SendAsync(EmailSendRequest request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(result);
        }
    }
}
