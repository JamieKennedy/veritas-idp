using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Veritas.Infrastructure.Database;

public class VeritasDbContextFactory : IDesignTimeDbContextFactory<VeritasDbContext>
{
    public VeritasDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<VeritasDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=VeritasDb;Username=postgres;Password=password");

        return new VeritasDbContext(optionsBuilder.Options);
    }
}