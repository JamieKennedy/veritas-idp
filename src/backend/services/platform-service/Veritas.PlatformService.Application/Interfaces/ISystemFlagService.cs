using FluentResults;

namespace Veritas.PlatformService.Application.Interfaces;

public interface ISystemFlagService
{
    public Task<Result<bool>> GetFlagValueAsync(string key);
    public Task<Result> SetFlagValueAsync(string key, bool value);
}
