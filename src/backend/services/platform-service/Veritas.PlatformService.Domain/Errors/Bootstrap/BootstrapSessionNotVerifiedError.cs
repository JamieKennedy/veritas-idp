using Veritas.Shared.Errors;

namespace Veritas.PlatformService.Domain.Errors.Bootstrap;

public sealed class BootstrapSessionNotVerifiedError : ValidationError
{
    public const string ErrorCode = "PLATFORM_BOOTSTRAP_SESSION_NOT_VERIFIED";

    public BootstrapSessionNotVerifiedError() : base(ErrorCode, "The bootstrap session is not ready for completion.")
    {
    }
}
