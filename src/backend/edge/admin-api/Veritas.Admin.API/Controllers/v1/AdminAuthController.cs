using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Veritas.Admin.API.Models.AdminAuth;
using Veritas.Shared.Http;
using Veritas.UserService.Application.Interfaces;

namespace Veritas.Admin.API.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin-auth")]
[ApiController]
public sealed class AdminAuthController : BaseController<AdminAuthController>
{
    private readonly IAdminUserService _adminUserService;

    public AdminAuthController(
        ILogger<AdminAuthController> logger,
        IAdminUserService adminUserService) : base(logger)
    {
        _adminUserService = adminUserService;
    }

    /// <summary>
    /// Signs in an administrator with email and password credentials.
    /// </summary>
    /// <param name="request">The login request containing administrator credentials.</param>
    /// <param name="cancellationToken">A token that cancels credential validation.</param>
    /// <returns>A safe administrator identity when authentication succeeds.</returns>
    [HttpPost("login")]
    [EnableRateLimiting("admin-login")]
    public async Task<IActionResult> Login([FromBody] AdminLoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return FailureResultMapper.UnauthorizedProblem("Invalid administrator credentials.");
        }

        var result = await _adminUserService.ValidateAdminCredentialsAsync(
            request.Email,
            request.Password,
            cancellationToken);

        if (result.IsFailed)
        {
            Logger.LogWarning("Rejected administrator login attempt.");
            return FailureResultMapper.ToProblemDetails(result);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.Value.Id.ToString()),
            new(ClaimTypes.Email, result.Value.Email),
            new(ClaimTypes.Name, result.Value.Name ?? result.Value.Email)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = false,
                IssuedUtc = DateTimeOffset.UtcNow
            });

        return Ok(new AdminLoginResponse(result.Value.Id, result.Value.Email, result.Value.Name));
    }

    /// <summary>
    /// Signs out the current administrator by clearing the admin authentication cookie.
    /// </summary>
    /// <returns>An empty success response.</returns>
    [HttpPost("logout")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok();
    }
}
