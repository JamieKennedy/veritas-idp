using Microsoft.EntityFrameworkCore;

namespace Veritas.UserService.Infrastructure.Database;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }
}
