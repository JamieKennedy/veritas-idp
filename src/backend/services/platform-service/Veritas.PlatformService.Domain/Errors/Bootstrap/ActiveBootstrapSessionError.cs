using Veritas.PlatformService.Domain.Errors.Base;

namespace Veritas.PlatformService.Domain.Errors.Bootstrap;

public class ActiveBootstrapSessionError : ConflictError
{
    public ActiveBootstrapSessionError() : base("An active session already exists.")
    {
    }
}