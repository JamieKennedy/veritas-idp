using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

using Veritas.Admin.API.Models.Setup;
using Veritas.PlatformService.Application.Interfaces;
using Veritas.PlatformService.Domain.Errors.Bootstrap;
using Veritas.Shared.Http;

namespace Veritas.Admin.API.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class BootstrapController : BaseController<BootstrapController>
{
    private const string BootstrapCookieName = "__Host-veritas-bootstrap";
    private readonly IBootstrapService _bootstrapService;

    public BootstrapController(
        ILogger<BootstrapController> logger,
        IBootstrapService bootstrapService) : base(logger)
    {
        _bootstrapService = bootstrapService;
    }

    /// <summary>
    /// Gets the current bootstrap status.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>The current bootstrap status for the installation.</returns>
    [HttpGet("status")]
    public async Task<IActionResult> Status(CancellationToken cancellationToken)
    {
        var result = await _bootstrapService.GetBootstrapStatus(cancellationToken);

        if (result.IsFailed)
        {
            return FailureResultMapper.ToProblemDetails(result);
        }

        return Ok(new BootstrapStatusResponse(
            result.Value.IsConfigured,
            result.Value.HasActiveBootstrap,
            result.Value.ActiveBootstrapExpiresAtUtc));
    }

    /// <summary>
    /// Starts first-admin bootstrap after validating the deployment bootstrap secret.
    /// </summary>
    /// <param name="request">The start request containing the first-admin email and bootstrap secret.</param>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>An HTTP result describing whether bootstrap start was accepted.</returns>
    [HttpPost("start")]
    [EnableRateLimiting("bootstrap-start")]
    public async Task<IActionResult> Start([FromBody] StartBootstrapRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.BootstrapSecret))
        {
            return FailureResultMapper.ValidationProblem("First administrator email and bootstrap secret are required.");
        }

        var result = await _bootstrapService.StartBootstrap(
            request.Email,
            request.BootstrapSecret,
            HttpContext.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);

        if (result.IsFailed)
        {
            return FailureResultMapper.ToProblemDetails(result);
        }

        Response.Cookies.Append(BootstrapCookieName, result.Value, CreateBootstrapCookieOptions());
        return Accepted();
    }

    /// <summary>
    /// Completes first-admin bootstrap using the active bootstrap cookie.
    /// </summary>
    /// <param name="request">The completion request containing first-admin password details.</param>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>An HTTP result describing whether bootstrap completion succeeded.</returns>
    [HttpPost("complete")]
    [EnableRateLimiting("bootstrap-complete")]
    public async Task<IActionResult> Complete([FromBody] CompleteBootstrapRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return FailureResultMapper.ValidationProblem("First administrator password is required.");
        }

        Request.Cookies.TryGetValue(BootstrapCookieName, out var sessionToken);
        var result = await _bootstrapService.CompleteBootstrap(
            sessionToken ?? string.Empty,
            request.Password,
            request.DisplayName,
            cancellationToken);

        if (result.IsFailed)
        {
            if (result.Errors.Any(error => error is ExpiredBootstrapSessionError))
            {
                ClearBootstrapCookie();
            }

            return FailureResultMapper.ToProblemDetails(result);
        }

        ClearBootstrapCookie();
        return Ok();
    }

    /// <summary>
    /// Creates secure cookie options for the short-lived bootstrap session token.
    /// </summary>
    /// <returns>Cookie options for bootstrap session storage.</returns>
    private static CookieOptions CreateBootstrapCookieOptions()
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            MaxAge = TimeSpan.FromMinutes(15)
        };
    }

    /// <summary>
    /// Clears the bootstrap cookie from the response.
    /// </summary>
    private void ClearBootstrapCookie()
    {
        Response.Cookies.Delete(BootstrapCookieName, new CookieOptions
        {
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/"
        });
    }

}
