using Veritas.PlatformService.Domain.Types;

namespace Veritas.PlatformService.Domain.Errors.Base;

public class ValidationError : BaseError
{
    protected ValidationError(string message) : base(EErrorCode.Validation, message)
    {
    }
}