using Microsoft.Extensions.Logging;

namespace Veritas.Infrastructure.Database.Repositories;

public class BaseRepository<T>
{
    protected readonly VeritasDbContext Context;
    protected readonly ILogger<T> Logger;
    
    public BaseRepository(VeritasDbContext context, ILogger<T> logger)
    {
        Context = context;
        Logger = logger;
    }
}