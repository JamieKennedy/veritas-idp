using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Veritas.Admin.API.Models.Setup;
using Veritas.MessagingService.Application.Services;
using Veritas.PlatformService.Application.Interfaces;
using Veritas.Shared.Http;

namespace Veritas.Admin.API.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/setup")]
[ApiController]
public sealed class SetupController(
    ILogger<SetupController> logger,
    IBootstrapService bootstrapService,
    ISmtpSetupStatus smtpSetupStatus) : BaseController<SetupController>(logger)
{
    /// <summary>
    /// Gets the current setup status for bootstrap and SMTP setup.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the request.</param>
    /// <returns>The current setup status.</returns>
    [HttpGet("status")]
    public async Task<IActionResult> Status(CancellationToken cancellationToken)
    {
        var bootstrap = await bootstrapService.GetBootstrapStatus(cancellationToken);
        if (bootstrap.IsFailed)
        {
            return FailureResultMapper.ToProblemDetails(bootstrap);
        }

        return Ok(new SetupStatusResponse(
            bootstrap.Value.IsConfigured,
            bootstrap.Value.HasActiveBootstrap,
            bootstrap.Value.ActiveBootstrapExpiresAtUtc,
            await smtpSetupStatus.IsSmtpConfiguredAsync(cancellationToken)));
    }
}
