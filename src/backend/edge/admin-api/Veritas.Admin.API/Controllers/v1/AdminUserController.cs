using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Veritas.Admin.API.Models.AdminUser;

namespace Veritas.Admin.API.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class AdminUserController : BaseController<AdminUserController>
{
    public AdminUserController(ILogger<AdminUserController> logger) : base(logger) { }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAdminUserRequest dto)
    {
        return Created("", dto);
    }
}
