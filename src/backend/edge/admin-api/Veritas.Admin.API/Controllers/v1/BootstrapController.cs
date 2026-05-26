using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Veritas.Admin.API.Models.Setup;
using Veritas.PlatformService.Application.Interfaces;

namespace Veritas.Admin.API.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class BootstrapController : BaseController<BootstrapController>
{
    private const string BootstrapCompletedFlagKey = "BOOTSTRAP_COMPLETED";
    private readonly IBootstrapService _bootstrapService;
    private readonly ISystemFlagService _systemFlagService;
    private readonly string _setupToken;

    public BootstrapController(
        ILogger<BootstrapController> logger,
        IConfiguration configuration,
        IBootstrapService bootstrapService,
        ISystemFlagService systemFlagService) : base(logger)
    {
        _setupToken = configuration["SETUP_TOKEN"] ?? throw new InvalidOperationException("SETUP_TOKEN is not configured.");
        _bootstrapService = bootstrapService;
        _systemFlagService = systemFlagService;
    }

    /// <summary>
    /// Completes platform bootstrap after validating the deployment setup token.
    /// </summary>
    /// <param name="bootstrapDto">The bootstrap request containing setup token and first-admin credentials.</param>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>An HTTP result describing whether bootstrap completion was accepted.</returns>
    [HttpPost]
    public async Task<IActionResult> Bootstrap([FromBody] BootstrapRequest bootstrapDto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(bootstrapDto.SetupToken))
        {
            return BadRequest("Setup token is required.");
        }

        if (!string.Equals(bootstrapDto.SetupToken, _setupToken, StringComparison.Ordinal))
        {
            return Unauthorized("Invalid setup token.");
        }

        if (bootstrapDto.AdminUser is null ||
            string.IsNullOrWhiteSpace(bootstrapDto.AdminUser.Email) ||
            string.IsNullOrWhiteSpace(bootstrapDto.AdminUser.Password))
        {
            return BadRequest("Admin user email and password are required.");
        }

        var result = await _systemFlagService.SetFlagValue(BootstrapCompletedFlagKey, true);

        if (result.IsFailed)
        {
            Logger.LogError("Failed to mark platform bootstrap as completed: {Errors}",
                string.Join(", ", result.Errors.Select(error => error.Message)));
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to complete platform bootstrap.");
        }

        return Ok();
    }

    /// <summary>
    /// Gets the current bootstrap status.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>The current bootstrap-completion status.</returns>
    [HttpGet("status")]
    public async Task<IActionResult> Status(CancellationToken cancellationToken)
    {
        var bootstrapRequired = await _bootstrapService.GetBootstrapStatus(cancellationToken);

        if (bootstrapRequired.IsFailed)
        {
            Logger.LogError("Failed to get bootstrap status: {Errors}",
                string.Join(", ", bootstrapRequired.Errors.Select(error => error.Message)));
            return StatusCode(StatusCodes.Status500InternalServerError, "Failed to get bootstrap status.");
        }

        return Ok(new BootstrapStatusResponse(IsBootstrapCompleted: !bootstrapRequired.Value));
    }
}
