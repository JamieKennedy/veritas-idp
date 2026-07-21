using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

using Veritas.MessagingService.Application.DataTransferObjects;
using Veritas.MessagingService.Application.Services;
using Veritas.MessagingService.Domain.Entities;
using Veritas.MessagingService.Domain.Errors;
using Veritas.MessagingService.Infrastructure.Database;

using Xunit;

namespace Veritas.MessagingService.Application.Tests;

public sealed class TemplateServiceTests
{
    [Fact]
    public async Task UpdateTemplateAsync_rejects_unknown_placeholders()
    {
        await using var context = CreateContext();
        context.EmailTemplates.Add(GlobalTemplate("admin.welcome"));
        await context.SaveChangesAsync();
        var service = new TemplateService(NullLogger<TemplateService>.Instance, context);

        var result = await service.UpdateTemplateAsync(
            "admin.welcome",
            new UpdateEmailTemplateDto(
                "Welcome {{User.Email}}",
                "<p>{{User.Password}}</p>",
                "{{User.Email}}",
                true),
            CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.IsType<UnknownTemplatePlaceholderError>(Assert.Single(result.Errors));
    }

    [Fact]
    public async Task ResolveTemplateAsync_prefers_tenant_template_and_falls_back_to_global()
    {
        await using var context = CreateContext();
        var tenantId = Guid.NewGuid();
        context.EmailTemplates.Add(GlobalTemplate("admin.welcome"));
        context.EmailTemplates.Add(new EmailTemplate
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            TemplateKey = "admin.welcome",
            Subject = "Tenant {{User.Email}}",
            HtmlBody = "<p>Tenant {{User.Email}}</p>",
            TextBody = "Tenant {{User.Email}}",
            IsEnabled = true,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        });
        await context.SaveChangesAsync();
        var service = new TemplateService(NullLogger<TemplateService>.Instance, context);

        var tenantResult = await service.ResolveTemplateAsync(tenantId, "admin.welcome", CancellationToken.None);
        var fallbackResult = await service.ResolveTemplateAsync(Guid.NewGuid(), "admin.welcome", CancellationToken.None);

        Assert.True(tenantResult.IsSuccess);
        Assert.Equal(tenantId, tenantResult.Value.TenantId);
        Assert.True(fallbackResult.IsSuccess);
        Assert.Null(fallbackResult.Value.TenantId);
    }

    [Fact]
    public async Task ResolveTemplateAsync_returns_not_found_for_missing_template()
    {
        await using var context = CreateContext();
        var service = new TemplateService(NullLogger<TemplateService>.Instance, context);

        var result = await service.ResolveTemplateAsync(null, "missing.template", CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.IsType<TemplateNotFoundError>(Assert.Single(result.Errors));
    }

    private static EmailTemplate GlobalTemplate(string key)
    {
        return new EmailTemplate
        {
            Id = Guid.NewGuid(),
            TenantId = null,
            TemplateKey = key,
            Subject = "Welcome {{User.Email}}",
            HtmlBody = "<p>Welcome {{User.Email}}</p>",
            TextBody = "Welcome {{User.Email}}",
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
}
