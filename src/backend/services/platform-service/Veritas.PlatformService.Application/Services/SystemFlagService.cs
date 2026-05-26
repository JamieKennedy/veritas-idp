using FluentResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Veritas.PlatformService.Application.Interfaces;
using Veritas.PlatformService.Application.Persistence;
using Veritas.PlatformService.Domain.Entities;

namespace Veritas.PlatformService.Application.Services;

public class SystemFlagService : BaseService<SystemFlagService>, ISystemFlagService
{
    private readonly IPlatformDbContext _dbContext;

    public SystemFlagService(ILogger<SystemFlagService> logger, IPlatformDbContext dbContext) : base(logger)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<Result<bool>> GetFlagValue(string key)
    {
        // TODO: get from redis cache
        var val = await _dbContext.SystemFlags
            .AsNoTracking()
            .Where(flag => flag.Key == key)
            .Select(flag => (bool?)flag.Value)
            .FirstOrDefaultAsync();

        return val ?? Result.Fail<bool>($"Flag with key '{key}' not found.");
    }

    /// <inheritdoc />
    public async Task<Result> SetFlagValue(string key, bool value)
    {
        var flag = await _dbContext.SystemFlags.FirstOrDefaultAsync(systemFlag => systemFlag.Key == key);

        if (flag is null)
        {
            _dbContext.SystemFlags.Add(new SystemFlag
            {
                Key = key,
                Value = value
            });
        }
        else
        {
            flag.Value = value;
        }

        await _dbContext.SaveChangesAsync();
        return Result.Ok();
    }
}
