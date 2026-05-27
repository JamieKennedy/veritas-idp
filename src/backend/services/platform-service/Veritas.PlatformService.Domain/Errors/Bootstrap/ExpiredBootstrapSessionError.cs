using Veritas.Shared.Errors;

namespace Veritas.PlatformService.Domain.Errors.Bootstrap;

public sealed class ExpiredBootstrapSessionError : ConflictError
{
    public const string ErrorCode = "PLATFORM_BOOTSTRAP_SESSION_EXPIRED";

    public ExpiredBootstrapSessionError() : base(ErrorCode, "The bootstrap session has expired.")
    {
        Metadata[ErrorMetadataKeys.HttpStatusCode] = "410";
    }
}
