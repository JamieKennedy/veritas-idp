using Veritas.Shared.Errors;

namespace Veritas.PlatformService.Domain.Errors.Bootstrap;

public sealed class InvalidBootstrapSessionError : UnauthorizedError
{
    public const string ErrorCode = "PLATFORM_BOOTSTRAP_SESSION_INVALID";

    public InvalidBootstrapSessionError() : base(ErrorCode, "The bootstrap session is invalid.")
    {
    }
}
