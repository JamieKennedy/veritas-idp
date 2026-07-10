using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Veritas.Admin.API.Models.AdminAuth;
using Veritas.Shared.Http;
using Veritas.UserService.Application.DataTransferObjects;
using Veritas.UserService.Application.Interfaces;

namespace Veritas.Admin.API.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin-auth")]
[ApiController]
public sealed class AdminAuthController : BaseController<AdminAuthController>
{
    /// <summary>
    /// Identifies the server-side admin session id claim in the admin auth cookie.
    /// </summary>
    public const string SessionIdClaimType = "veritas_admin_session_id";

    /// <summary>
    /// Identifies the admin security stamp claim in the admin auth cookie.
    /// </summary>
    public const string SecurityStampClaimType = "veritas_admin_security_stamp";

    private readonly IAdminUserService _adminUserService;
    private readonly IAntiforgery _antiforgery;

    public AdminAuthController(
        ILogger<AdminAuthController> logger,
        IAdminUserService adminUserService,
        IAntiforgery antiforgery) : base(logger)
    {
        _adminUserService = adminUserService;
        _antiforgery = antiforgery;
    }

    /// <summary>
    /// Creates and stores an antiforgery token for cookie-authenticated Admin API requests.
    /// </summary>
    /// <returns>The antiforgery request token clients must send in the X-CSRF-TOKEN header.</returns>
    [HttpGet("csrf")]
    public IActionResult GetCsrfToken()
    {
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
        return Ok(new AdminCsrfTokenResponse(tokens.RequestToken ?? string.Empty));
    }

    /// <summary>
    /// Gets the safe identity of the currently authenticated administrator.
    /// </summary>
    /// <returns>The authenticated administrator identity, or an unauthorized response when required claims are absent.</returns>
    [HttpGet("me")]
    [Authorize]
    public IActionResult CurrentAdmin()
    {
        var adminUserIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);

        if (!Guid.TryParse(adminUserIdClaim, out var adminUserId) || string.IsNullOrWhiteSpace(email))
        {
            return Unauthorized();
        }

        return Ok(new AdminLoginResponse(
            adminUserId,
            email,
            User.FindFirstValue(ClaimTypes.Name)));
    }

    /// <summary>
    /// Validates administrator credentials and returns the MFA challenge required to complete login.
    /// </summary>
    /// <param name="request">The login request containing administrator credentials.</param>
    /// <param name="cancellationToken">A token that cancels credential validation.</param>
    /// <returns>A short-lived MFA challenge when password validation succeeds.</returns>
    [HttpPost("login")]
    [EnableRateLimiting("admin-login")]
    public async Task<IActionResult> Login([FromBody] AdminLoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return FailureResultMapper.UnauthorizedProblem("Invalid administrator credentials.");
        }

        var result = await _adminUserService.StartAdminLoginAsync(
            request.Email,
            request.Password,
            cancellationToken);

        if (result.IsFailed)
        {
            Logger.LogWarning("Rejected administrator login attempt.");
            return FailureResultMapper.ToProblemDetails(result);
        }

        return StatusCode(
            StatusCodes.Status202Accepted,
            new AdminLoginChallengeResponse(
                result.Value.ChallengeId,
                result.Value.ChallengeToken,
                result.Value.Purpose,
                result.Value.ExpiresAtUtc,
                result.Value.TotpSecretBase32,
                result.Value.TotpProvisioningUri));
    }

    /// <summary>
    /// Confirms first-login administrator TOTP enrollment and signs in the administrator.
    /// </summary>
    /// <param name="request">The MFA enrollment confirmation request.</param>
    /// <param name="cancellationToken">A token that cancels enrollment completion.</param>
    /// <returns>The authenticated administrator and one-time recovery codes.</returns>
    [HttpPost("mfa/enroll/confirm")]
    [EnableRateLimiting("admin-login")]
    public async Task<IActionResult> CompleteMfaEnrollment(
        [FromBody] AdminMfaEnrollmentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _adminUserService.CompleteAdminMfaEnrollmentAsync(
            request.ChallengeId,
            request.ChallengeToken,
            request.TotpCode,
            cancellationToken);

        if (result.IsFailed)
        {
            Logger.LogWarning("Rejected administrator MFA enrollment attempt.");
            return FailureResultMapper.ToProblemDetails(result);
        }

        await SignInAdminSessionAsync(result.Value, cancellationToken);

        return Ok(new AdminMfaEnrollmentResponse(
            ToLoginResponse(result.Value.Admin),
            result.Value.RecoveryCodes));
    }

    /// <summary>
    /// Verifies administrator MFA and signs in the administrator.
    /// </summary>
    /// <param name="request">The MFA verification request.</param>
    /// <param name="cancellationToken">A token that cancels MFA verification.</param>
    /// <returns>The authenticated administrator identity.</returns>
    [HttpPost("mfa/verify")]
    [EnableRateLimiting("admin-login")]
    public async Task<IActionResult> CompleteMfaVerification(
        [FromBody] AdminMfaVerificationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _adminUserService.CompleteAdminMfaVerificationAsync(
            request.ChallengeId,
            request.ChallengeToken,
            request.Code,
            cancellationToken);

        if (result.IsFailed)
        {
            Logger.LogWarning("Rejected administrator MFA verification attempt.");
            return FailureResultMapper.ToProblemDetails(result);
        }

        await SignInAdminSessionAsync(result.Value, cancellationToken);

        return Ok(ToLoginResponse(result.Value.Admin));
    }

    /// <summary>
    /// Signs out the current administrator by revoking the server-side admin session and clearing the cookie.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels session revocation.</param>
    /// <returns>An empty success response.</returns>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var adminUserIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var sessionIdClaim = User.FindFirstValue(SessionIdClaimType);

        if (Guid.TryParse(adminUserIdClaim, out var adminUserId) &&
            Guid.TryParse(sessionIdClaim, out var sessionId))
        {
            var revokeResult = await _adminUserService.RevokeAdminSessionAsync(
                adminUserId,
                sessionId,
                "logout",
                cancellationToken);
            if (revokeResult.IsFailed)
            {
                Logger.LogWarning("Failed to revoke administrator session during logout.");
                return FailureResultMapper.ToProblemDetails(revokeResult);
            }
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok();
    }

    /// <summary>
    /// Issues the browser auth cookie for a fully authenticated administrator session.
    /// </summary>
    /// <param name="session">The authenticated administrator session.</param>
    /// <param name="cancellationToken">A token that cancels cookie issuance.</param>
    /// <returns>A task that completes when the cookie has been issued.</returns>
    private async Task SignInAdminSessionAsync(
        AdminSessionAuthenticationDto session,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, session.Admin.Id.ToString()),
            new(ClaimTypes.Email, session.Admin.Email),
            new(SessionIdClaimType, session.SessionId.ToString()),
            new(SecurityStampClaimType, session.SecurityStamp.ToString())
        };
        if (!string.IsNullOrWhiteSpace(session.Admin.Name))
        {
            claims.Add(new Claim(ClaimTypes.Name, session.Admin.Name));
        }
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = false,
                IssuedUtc = DateTimeOffset.UtcNow,
                ExpiresUtc = new DateTimeOffset(session.IdleExpiresAtUtc, TimeSpan.Zero)
            });
    }

    /// <summary>
    /// Maps a Users Application administrator DTO to the Admin API login response.
    /// </summary>
    /// <param name="admin">The safe administrator identity.</param>
    /// <returns>The Admin API login response.</returns>
    private static AdminLoginResponse ToLoginResponse(AdminUserAuthenticationDto admin)
    {
        return new AdminLoginResponse(admin.Id, admin.Email, admin.Name);
    }
}
