using FluentResults;

namespace Veritas.Admin.Application.Interfaces;

public interface ISystemFlagService
{
    public Task<Result<bool>> GetFlagValue(string key);
}