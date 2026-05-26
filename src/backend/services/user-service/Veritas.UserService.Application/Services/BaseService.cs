using Microsoft.Extensions.Logging;

namespace Veritas.UserService.Application.Services;

public abstract class BaseService<T>(
    ILogger<T> logger)
{
    protected ILogger<T> Logger { get; } = logger;
}
