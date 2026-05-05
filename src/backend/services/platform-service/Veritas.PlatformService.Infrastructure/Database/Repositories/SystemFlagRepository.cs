using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Veritas.PlatformService.Domain.Repositories;

namespace Veritas.PlatformService.Infrastructure.Database.Repositories;

public class SystemFlagRepository : BaseRepository<SystemFlagRepository>, ISystemFlagRepository
{
    public SystemFlagRepository(PlatformDbContext context, ILogger<SystemFlagRepository> logger) : base(context, logger)
    {
    }


    public async Task<bool?> GetFlagValue(string key)
    {
        var flag = await Context.SystemFlags.FirstOrDefaultAsync(f => f.Key == key);
        return flag?.Value;
    }

    public async Task SetFlagValue(string key, bool value)
    {
        var flag = await Context.SystemFlags.FirstOrDefaultAsync(f => f.Key == key);

        if (flag is null)
        {
            Context.SystemFlags.Add(new PlatformService.Domain.Entities.SystemFlag
            {
                Key = key,
                Value = value
            });
        }
        else
        {
            flag.Value = value;
        }

        await Context.SaveChangesAsync();
    }
}
