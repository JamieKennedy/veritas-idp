namespace Veritas.PlatformService.Domain.Repositories;

public interface ISystemFlagRepository
{
    Task<bool?> GetFlagValue(string key);
    Task SetFlagValue(string key, bool value);
}
