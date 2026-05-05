using Microsoft.EntityFrameworkCore;

namespace Veritas.PlatformService.Infrastructure.Database;

public static class PlatformDbContextOptions
{
    public const string MigrationHistoryTable = "__EFMigrationsHistory_Platform";

    public static void Configure(DbContextOptionsBuilder optionsBuilder, string? connectionString = null)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            optionsBuilder.UseNpgsql(npgsql =>
                npgsql.MigrationsHistoryTable(MigrationHistoryTable));
            return;
        }

        optionsBuilder.UseNpgsql(
            connectionString,
            npgsql => npgsql.MigrationsHistoryTable(MigrationHistoryTable));
    }
}
