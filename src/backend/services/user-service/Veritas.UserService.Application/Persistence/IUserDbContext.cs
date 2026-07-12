using Microsoft.EntityFrameworkCore;

using Veritas.UserService.Domain.Entities;

namespace Veritas.UserService.Application.Persistence;

/// <summary>
/// Exposes Users-owned persistence to Users application use cases.
/// </summary>
public interface IUserDbContext
{
    /// <summary>
    /// Gets admin identity records, including credential hashes.
    /// </summary>
    DbSet<AdminUser> AdminUsers
    {
        get;
    }

    /// <summary>
    /// Gets short-lived admin login challenges that gate MFA completion before cookie issuance.
    /// </summary>
    DbSet<AdminLoginChallenge> AdminLoginChallenges
    {
        get;
    }

    /// <summary>
    /// Gets server-side administrator sessions that back browser authentication cookies.
    /// </summary>
    DbSet<AdminSession> AdminSessions
    {
        get;
    }

    /// <summary>
    /// Gets hashed one-time administrator MFA recovery codes.
    /// </summary>
    DbSet<AdminRecoveryCode> AdminRecoveryCodes
    {
        get;
    }

    /// <summary>
    /// Persists pending Users module changes as one atomic unit of work.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the save operation.</param>
    /// <returns>The number of state entries written to the database.</returns>
    /// <exception cref="DbUpdateException">Thrown when the database rejects the update.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken" /> is canceled.</exception>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
