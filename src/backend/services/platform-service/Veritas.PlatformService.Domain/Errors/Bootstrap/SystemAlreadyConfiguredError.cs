using Veritas.Shared.Errors;

namespace Veritas.PlatformService.Domain.Errors.Bootstrap;

public class SystemAlreadyConfiguredError : ConflictError
{
    public const string ErrorCode = "PLATFORM_SYSTEM_ALREADY_CONFIGURED";

    public SystemAlreadyConfiguredError() : base(ErrorCode, "The system has already been configured.")
    {
    }
}
