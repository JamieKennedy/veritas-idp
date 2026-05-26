using Microsoft.EntityFrameworkCore;
using Veritas.PlatformService.Domain.Entities;

namespace Veritas.PlatformService.Application.Persistence;

/// <summary>
/// Exposes Platform-owned persistence to Platform application use cases.
/// </summary>
public interface IPlatformDbContext
{
    /// <summary>
    /// Gets the durable platform bootstrap sessions.
    /// </summary>
    DbSet<BootstrapSession> BootstrapSessions { get; }

    /// <summary>
    /// Gets platform-owned system flags.
    /// </summary>
    DbSet<SystemFlag> SystemFlags { get; }

    /// <summary>
    /// Persists pending Platform module changes as one atomic unit of work.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the save operation.</param>
    /// <returns>The number of state entries written to the database.</returns>
    /// <exception cref="DbUpdateException">Thrown when the database rejects the update.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken" /> is canceled.</exception>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
