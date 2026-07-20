using System.Text.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

using Veritas.MessagingService.Application.Services;
using Veritas.MessagingService.Domain.Entities;
using Veritas.MessagingService.Domain.Types;
using Veritas.MessagingService.Infrastructure.Database;
using Veritas.MessagingService.Infrastructure.Email;
using Veritas.Shared.Security;

using Xunit;

namespace Veritas.MessagingService.Application.Tests;

public sealed class MailpitIntegrationTests
{
    [Fact]
    public async Task SendTemplatedEmailAsync_delivers_to_local_mailpit_when_enabled()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("VERITAS_RUN_SMTP_INTEGRATION"), "1", StringComparison.Ordinal))
        {
            return;
        }

        var unique = Guid.NewGuid().ToString("N");
        var recipient = $"veritas-integration-{unique}@example.com";
        await using var context = CreateContext(unique);
        var service = new EmailDeliveryService(
            NullLogger<EmailDeliveryService>.Instance,
            context,
            new PassthroughSecretProtector(),
            new TemplateRenderer(),
            new SmtpEmailSender(NullLogger<SmtpEmailSender>.Instance));

        var result = await service.SendTemplatedEmailAsync(
            null,
            recipient,
            "admin.welcome",
            JsonSerializer.Serialize(new
            {
                User = new
                {
                    Email = recipient,
                    DisplayName = "Integration Test"
                },
                Action = new
                {
                    Url = $"https://veritas.example/integration/{unique}"
                }
            }),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        using var httpClient = new HttpClient();
        using var response = await httpClient.GetAsync("http://localhost:8025/api/v1/messages", CancellationToken.None);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(CancellationToken.None);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: CancellationToken.None);
        var messages = document.RootElement.GetProperty("messages").EnumerateArray();

        var matchingMessage = Assert.Single(
            messages,
            message => message.GetProperty("Subject").GetString() == $"Veritas integration {recipient}");
        var messageId = matchingMessage.GetProperty("ID").GetString();
        using var detailResponse = await httpClient.GetAsync($"http://localhost:8025/api/v1/message/{messageId}", CancellationToken.None);
        detailResponse.EnsureSuccessStatusCode();
        await using var detailStream = await detailResponse.Content.ReadAsStreamAsync(CancellationToken.None);
        using var detailDocument = await JsonDocument.ParseAsync(detailStream, cancellationToken: CancellationToken.None);

        Assert.Contains("<p>Integration Test</p>", detailDocument.RootElement.GetProperty("HTML").GetString(), StringComparison.Ordinal);
        Assert.Contains("Integration Test", detailDocument.RootElement.GetProperty("Text").GetString(), StringComparison.Ordinal);
        Assert.DoesNotContain("<p>", detailDocument.RootElement.GetProperty("Text").GetString(), StringComparison.Ordinal);
    }

    private static MessagingDbContext CreateContext(string unique)
    {
        var options = new DbContextOptionsBuilder<MessagingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        var context = new MessagingDbContext(options);
        context.SmtpSettings.Add(new SmtpSettings
        {
            Id = SmtpSettings.SingletonId,
            Host = "localhost",
            Port = 1025,
            TlsMode = SmtpTlsMode.None,
            FromEmail = "noreply@example.com",
            FromName = "Veritas",
            IsConfigured = true,
            LastSuccessfulTestAtUtc = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        });
        context.EmailTemplates.Add(new EmailTemplate
        {
            Id = Guid.NewGuid(),
            TemplateKey = "admin.welcome",
            Subject = "Veritas integration {{User.Email}}",
            HtmlBody = "<p>{{User.DisplayName}}</p><p>{{Action.Url}}</p>",
            TextBody = "{{User.DisplayName}} {{Action.Url}}",
            IsEnabled = true,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        });
        context.SaveChanges();
        return context;
    }

    private sealed class PassthroughSecretProtector : ISecretProtector
    {
        public string Protect(string plaintext)
        {
            return plaintext;
        }

        public string Unprotect(string protectedText)
        {
            return protectedText;
        }
    }
}
