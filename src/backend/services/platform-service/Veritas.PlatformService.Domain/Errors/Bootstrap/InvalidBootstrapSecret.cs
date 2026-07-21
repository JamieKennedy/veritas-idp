using Veritas.Shared.Errors;

namespace Veritas.PlatformService.Domain.Errors.Bootstrap;

public class InvalidBootstrapSecret : UnauthorizedError
{
    public const string ErrorCode = "PLATFORM_BOOTSTRAP_SECRET_INVALID";

    public InvalidBootstrapSecret() : base(ErrorCode, "The provided bootstrap secret is invalid.")
    {
    }
}
