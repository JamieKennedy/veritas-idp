using Microsoft.EntityFrameworkCore;

namespace Veritas.UserService.Infrastructure.Database;

public static class UserDbContextOptions
{
    public const string MigrationHistoryTable = "__EFMigrationsHistory_User";

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
