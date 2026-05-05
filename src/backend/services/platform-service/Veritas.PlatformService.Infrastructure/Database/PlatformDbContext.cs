using Microsoft.EntityFrameworkCore;
using Veritas.PlatformService.Domain.Entities;

namespace Veritas.PlatformService.Infrastructure.Database;

public class PlatformDbContext : DbContext
{    
    public PlatformDbContext(DbContextOptions<PlatformDbContext> options) : base(options)
    {
    }
    
    public DbSet<SystemFlag> SystemFlags { get; set; }
}
