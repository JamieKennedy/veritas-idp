using Microsoft.Extensions.Logging;

namespace Veritas.PlatformService.Infrastructure.Database.Repositories;

public class BaseRepository<T>
{
    protected readonly PlatformDbContext Context;
    protected readonly ILogger<T> Logger;
    
    public BaseRepository(PlatformDbContext context, ILogger<T> logger)
    {
        Context = context;
        Logger = logger;
    }
}
