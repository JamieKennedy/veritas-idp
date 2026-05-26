using Veritas.PlatformService.Domain.Types;

namespace Veritas.PlatformService.Domain.Errors.Base;

public class UnauthorizedError : BaseError
{
    public UnauthorizedError(string message): base(EErrorCode.Unauthorized, message)
    {
    }
}