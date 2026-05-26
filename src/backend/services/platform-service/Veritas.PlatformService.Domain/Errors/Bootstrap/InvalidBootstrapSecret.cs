using Veritas.PlatformService.Domain.Errors.Base;

namespace Veritas.PlatformService.Domain.Errors.Bootstrap;

public class InvalidBootstrapSecret : UnauthorizedError
{
    public InvalidBootstrapSecret() : base("The provided bootstrap secret is invalid.")
    {
    }
}