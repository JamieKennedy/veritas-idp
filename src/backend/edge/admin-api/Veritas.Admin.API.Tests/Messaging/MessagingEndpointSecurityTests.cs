using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Veritas.Admin.API.Controllers.v1;
using Veritas.Admin.API.Middleware;
using Veritas.MessagingService.Application.Services;
using Xunit;

namespace Veritas.Admin.API.Tests.Messaging;

public sealed class MessagingEndpointSecurityTests
{
    [Fact]
    public void MessagingController_requires_authenticated_administrator()
    {
        var authorizeAttribute = typeof(MessagingController).GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(authorizeAttribute);
    }

    [Fact]
    public async Task SmtpSetupGateMiddleware_blocks_protected_admin_paths_until_smtp_is_configured()
    {
        var nextCalled = false;
        var middleware = new SmtpSetupGateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v1/admin-users";
        context.User = CreateAuthenticatedAdmin();

        await middleware.InvokeAsync(context, new StubSmtpSetupStatus(false));

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status428PreconditionRequired, context.Response.StatusCode);
    }

    [Theory]
    [InlineData("/api/v1/setup/status")]
    [InlineData("/api/v1/messaging/settings")]
    [InlineData("/api/v1/messaging/settings/smtp")]
    [InlineData("/api/v1/messaging/templates")]
    [InlineData("/api/v1/admin-auth/logout")]
    public async Task SmtpSetupGateMiddleware_allows_setup_messaging_and_logout_paths_before_smtp_is_configured(string path)
    {
        var nextCalled = false;
        var middleware = new SmtpSetupGateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.User = CreateAuthenticatedAdmin();

        await middleware.InvokeAsync(context, new StubSmtpSetupStatus(false));

        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task SmtpSetupGateMiddleware_allows_protected_paths_after_smtp_is_configured()
    {
        var nextCalled = false;
        var middleware = new SmtpSetupGateMiddleware(_ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });
        var context = new DefaultHttpContext();
        context.Request.Path = "/api/v1/admin-users";
        context.User = CreateAuthenticatedAdmin();

        await middleware.InvokeAsync(context, new StubSmtpSetupStatus(true));

        Assert.True(nextCalled);
    }

    private sealed class StubSmtpSetupStatus(bool isConfigured) : ISmtpSetupStatus
    {
        public Task<bool> IsSmtpConfiguredAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(isConfigured);
        }
    }

    private static ClaimsPrincipal CreateAuthenticatedAdmin()
    {
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())],
            CookieAuthenticationDefaults.AuthenticationScheme);

        return new ClaimsPrincipal(identity);
    }
}
