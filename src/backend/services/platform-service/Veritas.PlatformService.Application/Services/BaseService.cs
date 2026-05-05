using Microsoft.Extensions.Logging;

namespace Veritas.PlatformService.Application.Services;

public class BaseService<T>
{
    protected readonly ILogger<T> Logger;
    
    protected BaseService(ILogger<T> logger)
    {
        Logger = logger;
    }
}
