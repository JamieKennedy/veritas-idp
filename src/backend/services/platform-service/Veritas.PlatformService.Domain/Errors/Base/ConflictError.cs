using Veritas.PlatformService.Domain.Types;

namespace Veritas.PlatformService.Domain.Errors.Base;

public class ConflictError : BaseError
{
    public ConflictError(string message) : base(EErrorCode.Conflict, message)
    {
    }
}