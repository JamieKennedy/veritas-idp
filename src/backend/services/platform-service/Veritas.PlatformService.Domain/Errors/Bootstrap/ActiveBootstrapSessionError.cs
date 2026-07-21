using Veritas.Shared.Errors;

namespace Veritas.PlatformService.Domain.Errors.Bootstrap;

public class ActiveBootstrapSessionError : ConflictError
{
    public const string ErrorCode = "PLATFORM_BOOTSTRAP_ACTIVE_SESSION_EXISTS";

    public ActiveBootstrapSessionError() : base(ErrorCode, "An active session already exists.")
    {
    }
}
