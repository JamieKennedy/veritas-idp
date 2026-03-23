using FluentResults;
using Microsoft.Extensions.Logging;
using Veritas.Admin.Application.Interfaces;
using Veritas.Domain.Repositories;

namespace Veritas.Admin.Application.Services;

public class SystemFlagService : BaseService<SystemFlagService>, ISystemFlagService
{
    private readonly ISystemFlagRepository _systemFlagRepository;
    
    public SystemFlagService(ILogger<SystemFlagService> logger, ISystemFlagRepository repository) : base(logger)
    {
        _systemFlagRepository = repository;
    }

    public async Task<Result<bool>> GetFlagValue(string key)
    {
        // TODO: get from redis cache

        var val = await _systemFlagRepository.GetFlagValue(key);
        
        return val ?? Result.Fail<bool>($"Flag with key '{key}' not found.");
    }
}