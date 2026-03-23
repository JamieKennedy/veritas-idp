using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Veritas.Domain.Repositories;

namespace Veritas.Infrastructure.Database.Repositories;

public class SystemFlagRepository : BaseRepository<SystemFlagRepository>, ISystemFlagRepository
{
    public SystemFlagRepository(VeritasDbContext context, ILogger<SystemFlagRepository> logger) : base(context, logger)
    {
    }


    public async Task<bool?> GetFlagValue(string key)
    {
        var flag = await Context.SystemFlags.FirstOrDefaultAsync(f => f.Key == key);
        return flag?.Value;
    }
}