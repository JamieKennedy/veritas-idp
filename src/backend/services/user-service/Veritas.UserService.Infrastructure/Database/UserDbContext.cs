using Microsoft.EntityFrameworkCore;

using Veritas.UserService.Application.Persistence;
using Veritas.UserService.Domain.Entities;

namespace Veritas.UserService.Infrastructure.Database;

public class UserDbContext : DbContext, IUserDbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Gets or sets admin identity records, including credential hashes and MFA state.
    /// </summary>
    public DbSet<AdminUser> AdminUsers
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets short-lived admin login challenges that gate MFA completion.
    /// </summary>
    public DbSet<AdminLoginChallenge> AdminLoginChallenges
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets server-side administrator sessions that back browser authentication cookies.
    /// </summary>
    public DbSet<AdminSession> AdminSessions
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets hashed one-time administrator MFA recovery codes.
    /// </summary>
    public DbSet<AdminRecoveryCode> AdminRecoveryCodes
    {
        get; set;
    }

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

        modelBuilder.Entity<AdminLoginChallenge>()
            .HasIndex(challenge => challenge.AdminUserId);

        modelBuilder.Entity<AdminLoginChallenge>()
            .HasIndex(challenge => challenge.ExpiresAtUtc);

        modelBuilder.Entity<AdminLoginChallenge>()
            .Property(challenge => challenge.ConsumedAtUtc)
            .IsConcurrencyToken();

        modelBuilder.Entity<AdminSession>()
            .HasIndex(session => new { session.AdminUserId, session.RevokedAtUtc });

        modelBuilder.Entity<AdminRecoveryCode>()
            .HasIndex(code => code.AdminUserId);

        modelBuilder.Entity<AdminRecoveryCode>()
            .Property(code => code.ConsumedAtUtc)
            .IsConcurrencyToken();
    }
}
