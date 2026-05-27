using FluentResults;

namespace Veritas.PlatformService.Application.Dependencies;

/// <summary>
/// Creates the first administrator account through the Users module boundary.
/// </summary>
public interface IInitialAdminCreator
{
    /// <summary>
    /// Creates the initial administrator account when the installation is not configured.
    /// </summary>
    /// <param name="email">The normalized email captured when bootstrap started.</param>
    /// <param name="password">The raw password supplied during bootstrap completion.</param>
    /// <param name="displayName">The optional administrator display name.</param>
    /// <param name="cancellationToken">A token that cancels the create operation.</param>
    /// <returns>A result that succeeds when the Users module creates the first administrator.</returns>
    Task<Result> CreateInitialAdminUserAsync(
        string email,
        string password,
        string? displayName,
        CancellationToken cancellationToken = default);
}
