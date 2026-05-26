using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Veritas.UserService.Infrastructure.Database;

public class UserContextFactory : IDesignTimeDbContextFactory<UserDbContext>
{
    public UserDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
        UserDbContextOptions.Configure(
            optionsBuilder,
            "Host=localhost;Database=VeritasDb;Username=postgres;Password=password");

        return new UserDbContext(optionsBuilder.Options);
    }
}
