using FluentResults;
using Veritas.PlatformService.Domain.Types;

namespace Veritas.PlatformService.Domain.Errors.Base;

public class BaseError : Error
{
    protected EErrorCode ErrorCode { get; set; }
    
    public BaseError(EErrorCode errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
        Metadata["ErrorCode"] = errorCode.ToString();
    }
}