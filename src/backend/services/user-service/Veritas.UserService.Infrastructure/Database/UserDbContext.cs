using Microsoft.EntityFrameworkCore;
using Veritas.UserService.Application.Persistence;
using Veritas.UserService.Domain.Entities;

namespace Veritas.UserService.Infrastructure.Database;

public class UserDbContext : DbContext, IUserDbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }

    public DbSet<AdminUser> AdminUsers { get; set; }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("users");

        modelBuilder.Entity<AdminUser>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<AdminUser>()
            .HasIndex(u => u.InitialAdminSlot)
            .IsUnique()
            .HasFilter("\"InitialAdminSlot\" IS NOT NULL");
    }
}
