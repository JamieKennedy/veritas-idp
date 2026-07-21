using Veritas.Shared.Errors;

namespace Veritas.PlatformService.Domain.Errors.Base;

/// <summary>
/// Result error from calling calling an external service.
/// </summary>
public class ExternalError : UnexpectedError
{
    public const string ErrorCode = "PLATFORM_EXTERNAL_ERROR";

    public ExternalError(string message) : base(ErrorCode, message)
    {
    }
}
