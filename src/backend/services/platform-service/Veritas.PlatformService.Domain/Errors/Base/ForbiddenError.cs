using Veritas.PlatformService.Domain.Types;

namespace Veritas.PlatformService.Domain.Errors.Base;

public class ForbiddenError : BaseError
{
    public ForbiddenError(string message) : base(EErrorCode.Forbidden, message)
    {
    }
}