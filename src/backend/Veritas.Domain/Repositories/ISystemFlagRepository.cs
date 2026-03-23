namespace Veritas.Domain.Repositories;

public interface ISystemFlagRepository
{
    Task<bool?> GetFlagValue(string key);
}