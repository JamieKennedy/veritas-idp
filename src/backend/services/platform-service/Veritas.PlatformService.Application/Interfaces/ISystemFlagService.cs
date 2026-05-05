using FluentResults;

namespace Veritas.PlatformService.Application.Interfaces;

public interface ISystemFlagService
{
    public Task<Result<bool>> GetFlagValue(string key);
    public Task<Result> SetFlagValue(string key, bool value);
}
