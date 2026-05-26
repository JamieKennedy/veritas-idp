using Veritas.PlatformService.Domain.Types;

namespace Veritas.PlatformService.Domain.Errors.Base;

/// <summary>
/// Result error from calling calling an external service.
/// </summary>
public class ExternalError : BaseError
{
    public ExternalError(string message) : base(EErrorCode.Unexpected, message)
    {
    }
}