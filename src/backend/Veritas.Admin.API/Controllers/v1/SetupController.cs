using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Veritas.Admin.Application.DataTransferObjects.Setup;
using Veritas.Admin.Application.Interfaces;

namespace Veritas.Admin.API.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class SetupController : BaseController<SetupController>
{
    private readonly string _setupToken;
    private readonly ISystemFlagService _systemFlagService;
    
    public SetupController(ILogger<SetupController> logger, IConfiguration configuration, ISystemFlagService systemFlagService) : base(logger)
    {
        _setupToken = configuration["SETUP_TOKEN"] ?? throw new InvalidOperationException("SETUP_TOKEN is not configured.");
        _systemFlagService = systemFlagService; 
    }

    [HttpPost]
    public async Task<IActionResult> Setup([FromBody] SetupDto setupDto)
    {
        if (string.IsNullOrWhiteSpace(setupDto.SetupToken))
        {
            return BadRequest("Setup token is required.");
        }

        if (!string.Equals(setupDto.SetupToken, _setupToken, StringComparison.Ordinal))
        {
            return Unauthorized("Invalid setup token.");
        }
        
        
        return Ok();
    }

    [HttpGet("status")]
    public async Task<IActionResult> Status()
    {
        var statusResult = await _systemFlagService.GetFlagValue("SETUP_COMPLETED");
        
        var isSetupCompleted = statusResult.IsSuccess && statusResult.Value;
        
        return Ok(isSetupCompleted);
    }
}