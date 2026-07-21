using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Veritas.MessagingService.Infrastructure.Database;

/// <summary>
/// Design-time factory for Messaging EF Core migrations.
/// </summary>
public sealed class MessagingContextFactory : IDesignTimeDbContextFactory<MessagingDbContext>
{
    /// <summary>
    /// Creates a Messaging DbContext for EF Core tooling.
    /// </summary>
    /// <param name="args">Command-line arguments passed by EF tooling.</param>
    /// <returns>A configured Messaging DbContext.</returns>
    public MessagingDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MessagingDbContext>()
            .UseNpgsql(
                "Host=localhost;Database=VeritasDb;Username=postgres;Password=postgres",
                npgsql => npgsql.MigrationsHistoryTable(MessagingDbContextOptions.MigrationsHistoryTable, "messaging"))
            .Options;

        return new MessagingDbContext(options);
    }
}
