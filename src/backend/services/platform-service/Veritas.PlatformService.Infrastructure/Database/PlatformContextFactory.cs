using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Veritas.PlatformService.Infrastructure.Database;

public class PlatformContextFactory : IDesignTimeDbContextFactory<PlatformDbContext>
{
    public PlatformDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PlatformDbContext>();
        PlatformDbContextOptions.Configure(
            optionsBuilder,
            "Host=localhost;Database=VeritasDb;Username=postgres;Password=password");

        return new PlatformDbContext(optionsBuilder.Options);
    }
}
