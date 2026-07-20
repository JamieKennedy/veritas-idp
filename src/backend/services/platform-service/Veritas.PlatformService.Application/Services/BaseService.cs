using Microsoft.Extensions.Logging;

namespace Veritas.PlatformService.Application.Services;

public class BaseService<T>
{
    protected ILogger<T> Logger
    {
        get;
    }

    protected BaseService(ILogger<T> logger)
    {
        Logger = logger;
    }
}
