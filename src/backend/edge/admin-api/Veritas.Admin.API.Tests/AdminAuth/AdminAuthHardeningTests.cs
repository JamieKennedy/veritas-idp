using System.Security.Claims;
using System.Text.Encodings.Web;
using FluentResults;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Veritas.Admin.API.Authentication;
using Veritas.Admin.API.Controllers.v1;
using Veritas.Admin.API.Models.AdminAuth;
using Veritas.UserService.Application.DataTransferObjects;
using Veritas.UserService.Application.Interfaces;
using Veritas.UserService.Domain.Entities;
using Xunit;

namespace Veritas.Admin.API.Tests.AdminAuth;

public sealed class AdminAuthHardeningTests
{
    [Fact]
    public async Task Login_returns_mfa_challenge_without_signing_in()
    {
        var authService = new RecordingAuthenticationService();
        var adminUserService = new StubAdminUserService
        {
            LoginChallenge = Result.Ok(new AdminLoginChallengeDto(
                Guid.NewGuid(),
                "challenge-token",
                EAdminLoginChallengePurpose.MfaEnrollment,
                DateTime.UtcNow.AddMinutes(5),
                "JBSWY3DPEHPK3PXP",
                "otpauth://totp/Veritas"))
        };
        var controller = CreateController(adminUserService, authService);

        var response = await controller.Login(new AdminLoginRequest
        {
            Email = "admin@example.com",
            Password = "Correct Horse Battery Staple 42!"
        }, CancellationToken.None);

        var accepted = Assert.IsType<ObjectResult>(response);
        Assert.Equal(StatusCodes.Status202Accepted, accepted.StatusCode);
        var body = Assert.IsType<AdminLoginChallengeResponse>(accepted.Value);
        Assert.Equal(EAdminLoginChallengePurpose.MfaEnrollment, body.Purpose);
        Assert.Equal("challenge-token", body.ChallengeToken);
        Assert.Equal(0, authService.SignInCount);
    }

    [Fact]
    public async Task CompleteMfaEnrollment_signs_in_with_session_claims_and_returns_recovery_codes()
    {
        var authService = new RecordingAuthenticationService();
        var adminId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var securityStamp = Guid.NewGuid();
        var adminUserService = new StubAdminUserService
        {
            EnrollmentResult = Result.Ok(new AdminMfaEnrollmentResultDto(
                new AdminUserAuthenticationDto(adminId, "admin@example.com", "First Admin"),
                sessionId,
                securityStamp,
                DateTime.UtcNow.AddMinutes(30),
                DateTime.UtcNow.AddHours(8),
                ["veritas-recovery-1"]))
        };
        var controller = CreateController(adminUserService, authService);

        var response = await controller.CompleteMfaEnrollment(new AdminMfaEnrollmentRequest(
            Guid.NewGuid(),
            "challenge-token",
            "123456"), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response);
        var body = Assert.IsType<AdminMfaEnrollmentResponse>(ok.Value);
        Assert.Equal(adminId, body.Admin.Id);
        Assert.Equal("veritas-recovery-1", Assert.Single(body.RecoveryCodes));
        Assert.Equal(1, authService.SignInCount);
        Assert.Equal(sessionId.ToString(), authService.SignedInPrincipal?.FindFirstValue(AdminAuthController.SessionIdClaimType));
        Assert.Equal(securityStamp.ToString(), authService.SignedInPrincipal?.FindFirstValue(AdminAuthController.SecurityStampClaimType));
    }

    [Fact]
    public async Task Logout_revokes_current_server_side_session_before_signing_out()
    {
        var authService = new RecordingAuthenticationService();
        var adminId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var adminUserService = new StubAdminUserService();
        var controller = CreateController(adminUserService, authService);
        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, adminId.ToString()),
                new Claim(AdminAuthController.SessionIdClaimType, sessionId.ToString())
            ],
            CookieAuthenticationDefaults.AuthenticationScheme));

        var response = await controller.Logout(CancellationToken.None);

        Assert.IsType<OkResult>(response);
        Assert.Equal(adminId, adminUserService.RevokedAdminUserId);
        Assert.Equal(sessionId, adminUserService.RevokedSessionId);
        Assert.Equal(1, authService.SignOutCount);
    }

    [Fact]
    public void GetCsrfToken_returns_request_token()
    {
        var antiforgery = new StubAntiforgery("csrf-token");
        var controller = CreateController(new StubAdminUserService(), new RecordingAuthenticationService(), antiforgery);

        var response = controller.GetCsrfToken();

        var ok = Assert.IsType<OkObjectResult>(response);
        var body = Assert.IsType<AdminCsrfTokenResponse>(ok.Value);
        Assert.Equal("csrf-token", body.Token);
        Assert.True(antiforgery.GetAndStoreTokensCalled);
    }

    [Fact]
    public async Task AdminCookieAuthenticationEvents_rejects_revoked_server_side_session()
    {
        var adminUserService = new StubAdminUserService
        {
            SessionValidation = Result.Fail<AdminSessionAuthenticationDto>("revoked")
        };
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                    new Claim(AdminAuthController.SessionIdClaimType, Guid.NewGuid().ToString()),
                    new Claim(AdminAuthController.SecurityStampClaimType, Guid.NewGuid().ToString())
                ],
                CookieAuthenticationDefaults.AuthenticationScheme))
        };
        context.RequestServices = new ServiceCollection()
            .AddSingleton<IAdminUserService>(adminUserService)
            .BuildServiceProvider();
        var ticket = new AuthenticationTicket(
            context.User,
            new AuthenticationProperties(),
            CookieAuthenticationDefaults.AuthenticationScheme);
        var options = Options.Create(new CookieAuthenticationOptions());
        var validateContext = new CookieValidatePrincipalContext(context, new AuthenticationScheme(
            CookieAuthenticationDefaults.AuthenticationScheme,
            CookieAuthenticationDefaults.AuthenticationScheme,
            typeof(CookieAuthenticationHandler)), options.Value, ticket);
        var events = new AdminCookieAuthenticationEvents();

        await events.ValidatePrincipal(validateContext);

        Assert.Null(validateContext.Principal);
    }

    private static AdminAuthController CreateController(
        IAdminUserService adminUserService,
        IAuthenticationService authenticationService,
        IAntiforgery? antiforgery = null)
    {
        var services = new ServiceCollection()
            .AddSingleton(authenticationService)
            .AddSingleton(antiforgery ?? new StubAntiforgery("csrf-token"))
            .BuildServiceProvider();
        var controller = new AdminAuthController(
            NullLogger<AdminAuthController>.Instance,
            adminUserService,
            services.GetRequiredService<IAntiforgery>())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    RequestServices = services
                }
            }
        };

        return controller;
    }

    private sealed class RecordingAuthenticationService : IAuthenticationService
    {
        public int SignInCount { get; private set; }

        public int SignOutCount { get; private set; }

        public ClaimsPrincipal? SignedInPrincipal { get; private set; }

        public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string? scheme)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        public Task ChallengeAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
        {
            return Task.CompletedTask;
        }

        public Task ForbidAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
        {
            return Task.CompletedTask;
        }

        public Task SignInAsync(
            HttpContext context,
            string? scheme,
            ClaimsPrincipal principal,
            AuthenticationProperties? properties)
        {
            SignInCount++;
            SignedInPrincipal = principal;
            return Task.CompletedTask;
        }

        public Task SignOutAsync(HttpContext context, string? scheme, AuthenticationProperties? properties)
        {
            SignOutCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class StubAntiforgery(string token) : IAntiforgery
    {
        public bool GetAndStoreTokensCalled { get; private set; }

        public AntiforgeryTokenSet GetAndStoreTokens(HttpContext httpContext)
        {
            GetAndStoreTokensCalled = true;
            return new AntiforgeryTokenSet(token, "cookie-token", "csrf", "X-CSRF-TOKEN");
        }

        public AntiforgeryTokenSet GetTokens(HttpContext httpContext)
        {
            return new AntiforgeryTokenSet(token, "cookie-token", "csrf", "X-CSRF-TOKEN");
        }

        public Task<bool> IsRequestValidAsync(HttpContext httpContext)
        {
            return Task.FromResult(true);
        }

        public Task ValidateRequestAsync(HttpContext httpContext)
        {
            return Task.CompletedTask;
        }

        public void SetCookieTokenAndHeader(HttpContext httpContext)
        {
        }
    }

    private sealed class StubAdminUserService : IAdminUserService
    {
        public Result<AdminLoginChallengeDto> LoginChallenge { get; init; } =
            Result.Fail<AdminLoginChallengeDto>("not configured");

        public Result<AdminMfaEnrollmentResultDto> EnrollmentResult { get; init; } =
            Result.Fail<AdminMfaEnrollmentResultDto>("not configured");

        public Result<AdminSessionAuthenticationDto> SessionValidation { get; init; } =
            Result.Ok(new AdminSessionAuthenticationDto(
                new AdminUserAuthenticationDto(Guid.NewGuid(), "admin@example.com", "First Admin"),
                Guid.NewGuid(),
                Guid.NewGuid(),
                DateTime.UtcNow.AddMinutes(30),
                DateTime.UtcNow.AddHours(8)));

        public Guid? RevokedAdminUserId { get; private set; }

        public Guid? RevokedSessionId { get; private set; }

        public Task<Result> CreateInitialAdminUserAsync(
            string email,
            string password,
            string? displayName,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result.Ok());
        }

        public Task<Result<AdminUserAuthenticationDto>> ValidateAdminCredentialsAsync(
            string email,
            string password,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result.Fail<AdminUserAuthenticationDto>("not used"));
        }

        public Task<Result<AdminLoginChallengeDto>> StartAdminLoginAsync(
            string email,
            string password,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(LoginChallenge);
        }

        public Task<Result<AdminMfaEnrollmentResultDto>> CompleteAdminMfaEnrollmentAsync(
            Guid challengeId,
            string challengeToken,
            string totpCode,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(EnrollmentResult);
        }

        public Task<Result<AdminSessionAuthenticationDto>> CompleteAdminMfaVerificationAsync(
            Guid challengeId,
            string challengeToken,
            string code,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result.Fail<AdminSessionAuthenticationDto>("not configured"));
        }

        public Task<Result<AdminSessionAuthenticationDto>> ValidateAdminSessionAsync(
            Guid adminUserId,
            Guid sessionId,
            Guid securityStamp,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(SessionValidation);
        }

        public Task<Result> RevokeAdminSessionAsync(
            Guid adminUserId,
            Guid sessionId,
            string reason,
            CancellationToken cancellationToken = default)
        {
            RevokedAdminUserId = adminUserId;
            RevokedSessionId = sessionId;
            return Task.FromResult(Result.Ok());
        }

        public Task<Result<int>> GetAdminUserCountAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result.Ok(1));
        }
    }
}
