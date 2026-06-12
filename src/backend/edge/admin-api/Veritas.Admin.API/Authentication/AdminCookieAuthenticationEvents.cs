using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Veritas.Admin.API.Controllers.v1;
using Veritas.UserService.Application.Interfaces;

namespace Veritas.Admin.API.Authentication;

/// <summary>
/// Validates admin authentication cookies against Users-owned server-side sessions.
/// </summary>
public sealed class AdminCookieAuthenticationEvents : CookieAuthenticationEvents
{
    /// <summary>
    /// Rejects cookies whose backing server-side admin session is no longer valid.
    /// </summary>
    /// <param name="context">The cookie principal validation context.</param>
    /// <returns>A task that completes when validation has finished.</returns>
    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var adminUserService = context.HttpContext.RequestServices.GetRequiredService<IAdminUserService>();
        var adminUserIdClaim = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var sessionIdClaim = context.Principal?.FindFirstValue(AdminAuthController.SessionIdClaimType);
        var securityStampClaim = context.Principal?.FindFirstValue(AdminAuthController.SecurityStampClaimType);

        if (!Guid.TryParse(adminUserIdClaim, out var adminUserId) ||
            !Guid.TryParse(sessionIdClaim, out var sessionId) ||
            !Guid.TryParse(securityStampClaim, out var securityStamp))
        {
            context.RejectPrincipal();
            return;
        }

        var validation = await adminUserService.ValidateAdminSessionAsync(
            adminUserId,
            sessionId,
            securityStamp,
            context.HttpContext.RequestAborted);
        if (validation.IsFailed)
        {
            context.RejectPrincipal();
        }
    }
}
