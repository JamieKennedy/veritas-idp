using Veritas.PlatformService.Domain.Types;

namespace Veritas.PlatformService.Domain.Errors.Base;

public class NotFoundError : BaseError
{
    public NotFoundError(string message) : base(EErrorCode.NotFound, message)
    {
    }
}