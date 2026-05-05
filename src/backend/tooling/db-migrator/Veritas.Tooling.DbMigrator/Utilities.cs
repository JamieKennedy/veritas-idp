using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Veritas.Tooling.DbMigrator;

public static class Utilities
{
    /// <summary>
    /// Migrates the database for the specified DbContext type. This method creates a scope, retrieves the DbContext from the service provider, and applies any pending migrations to the database.
    /// </summary>
    /// <param name="services"></param>
    /// <typeparam name="TContext"></typeparam>
    public static async Task MigrateAsync<TContext>(IServiceProvider services) where TContext : DbContext
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TContext>();

        await dbContext.Database.MigrateAsync();
    }
}