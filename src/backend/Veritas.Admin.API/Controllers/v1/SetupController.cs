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
    private readonly IConfiguration _configuration;
    private readonly ISystemFlagService _systemFlagService;

    private string SetupToken => _configuration["SETUP_TOKEN"] ?? throw new InvalidOperationException("SETUP_TOKEN is not configured.");
    
    public SetupController(ILogger<SetupController> logger, IConfiguration configuration, ISystemFlagService systemFlagService) : base(logger)
    {
        _configuration = configuration;
        _systemFlagService = systemFlagService; 
    }

    [HttpPost]
    public async Task<IActionResult> Setup([FromBody] SetupDto setupDto)
    {
        if(setupDto.SetupToken != SetupToken) return Unauthorized("Invalid setup token.");
        
        
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