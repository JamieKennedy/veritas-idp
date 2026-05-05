using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Veritas.Admin.API.Gateways;
using Veritas.Admin.API.Models.Setup;

namespace Veritas.Admin.API.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class SetupController : BaseController<SetupController>
{
    private readonly string _setupToken;
    private readonly PlatformSetupGateway _platformSetupGateway;
    
    public SetupController(ILogger<SetupController> logger, IConfiguration configuration, PlatformSetupGateway platformSetupGateway) : base(logger)
    {
        _setupToken = configuration["SETUP_TOKEN"] ?? throw new InvalidOperationException("SETUP_TOKEN is not configured.");
        _platformSetupGateway = platformSetupGateway;
    }

    [HttpPost]
    public async Task<IActionResult> Setup([FromBody] SetupRequest setupDto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(setupDto.SetupToken))
        {
            return BadRequest("Setup token is required.");
        }

        if (!string.Equals(setupDto.SetupToken, _setupToken, StringComparison.Ordinal))
        {
            return Unauthorized("Invalid setup token.");
        }

        try
        {
            await _platformSetupGateway.CompleteSetupAsync(setupDto, cancellationToken);
            return Ok();
        }
        catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.InvalidArgument)
        {
            return BadRequest(ex.Status.Detail);
        }
        catch (Grpc.Core.RpcException ex)
        {
            Logger.LogError(ex, "Platform setup gRPC call failed.");
            return base.StatusCode(StatusCodes.Status502BadGateway, "Platform service is unavailable.");
        }
    }

    [HttpGet("status")]
    public async Task<IActionResult> Status(CancellationToken cancellationToken)
    {
        try
        {
            var isSetupCompleted = await _platformSetupGateway.GetSetupStatusAsync(cancellationToken);
            return Ok(isSetupCompleted);
        }
        catch (Grpc.Core.RpcException ex)
        {
            Logger.LogError(ex, "Platform setup status gRPC call failed.");
            return base.StatusCode(StatusCodes.Status502BadGateway, "Platform service is unavailable.");
        }
    }
}
