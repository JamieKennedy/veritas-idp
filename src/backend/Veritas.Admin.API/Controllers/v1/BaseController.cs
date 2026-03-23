using Microsoft.AspNetCore.Mvc;

namespace Veritas.Admin.API.Controllers.v1;

public class BaseController<T> : ControllerBase
{
    protected ILogger<T> Logger;
    
    public BaseController(ILogger<T> logger)
    {
        Logger = logger;
    }
}