using FluentResults;
using Veritas.PlatformService.Domain.Errors.Base;
using Veritas.PlatformService.Domain.Types;

namespace Veritas.PlatformService.Domain.Errors.Bootstrap;

public class SystemAlreadyConfiguredError : ValidationError
{
    public SystemAlreadyConfiguredError() : base("The system has already been configured.")
    {
    }
}