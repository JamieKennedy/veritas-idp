using System.Text.Json;
using Veritas.MessagingService.Application.Services;
using Veritas.MessagingService.Domain.Entities;
using Veritas.MessagingService.Domain.Errors;
using Xunit;

namespace Veritas.MessagingService.Application.Tests;

public sealed class TemplateRendererTests
{
    [Fact]
    public void Render_replaces_allowed_placeholders_from_model_json()
    {
        var renderer = new TemplateRenderer();
        var template = Template("Welcome {{User.Email}}", "<p>Hello {{User.DisplayName}}</p>", "Hi {{User.Email}}");
        var model = JsonDocument.Parse("""{"User":{"Email":"admin@example.com","DisplayName":"First Admin"}}""");

        var result = renderer.Render(template, model.RootElement);

        Assert.True(result.IsSuccess);
        Assert.Equal("Welcome admin@example.com", result.Value.Subject);
        Assert.Equal("<p>Hello First Admin</p>", result.Value.HtmlBody);
        Assert.Equal("Hi admin@example.com", result.Value.TextBody);
    }

    [Fact]
    public void Render_rejects_model_that_is_missing_required_placeholder_value()
    {
        var renderer = new TemplateRenderer();
        var template = Template("Welcome {{User.Email}}", "<p>Hello {{User.DisplayName}}</p>", "Hi {{User.Email}}");
        var model = JsonDocument.Parse("""{"User":{"Email":"admin@example.com"}}""");

        var result = renderer.Render(template, model.RootElement);

        Assert.True(result.IsFailed);
        Assert.IsType<InvalidTemplateModelError>(Assert.Single(result.Errors));
    }

    private static EmailTemplate Template(string subject, string htmlBody, string textBody)
    {
        return new EmailTemplate
        {
            Id = Guid.NewGuid(),
            TemplateKey = "admin.welcome",
            Subject = subject,
            HtmlBody = htmlBody,
            TextBody = textBody,
            IsEnabled = true,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
    }
}
