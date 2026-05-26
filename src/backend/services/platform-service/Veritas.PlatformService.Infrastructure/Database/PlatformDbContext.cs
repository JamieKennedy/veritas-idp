using Microsoft.EntityFrameworkCore;
using Veritas.PlatformService.Application.Persistence;
using Veritas.PlatformService.Domain.Entities;

namespace Veritas.PlatformService.Infrastructure.Database;

public class PlatformDbContext : DbContext, IPlatformDbContext
{
    public PlatformDbContext(DbContextOptions<PlatformDbContext> options) : base(options)
    {
    }

    public DbSet<SystemFlag> SystemFlags { get; set; }
    public DbSet<BootstrapSession> BootstrapSessions { get; set; }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("platform");
    }
}
