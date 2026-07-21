using FluentResults;

namespace Veritas.PlatformService.Application.Dependencies;

/// <summary>
/// Provides the Platform module with the admin-user state it needs from the Users module.
/// </summary>
public interface IAdminUserDirectory
{
    /// <summary>
    /// Determines whether at least one admin user exists.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the lookup.</param>
    /// <returns>A result containing <see langword="true" /> when an admin user exists.</returns>
    Task<Result<bool>> HasAnyAdminUserAsync(CancellationToken cancellationToken = default);
}
