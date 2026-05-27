using FluentResults;
using Veritas.UserService.Application.DataTransferObjects;

namespace Veritas.UserService.Application.Interfaces;

public interface IAdminUserService
{
    /// <summary>
    /// Creates the first administrator account when no administrator exists.
    /// </summary>
    /// <param name="email">The administrator email address, which is normalized before persistence.</param>
    /// <param name="password">The raw password supplied during bootstrap. The value is hashed before storage.</param>
    /// <param name="displayName">The optional administrator display name.</param>
    /// <param name="cancellationToken">A token that cancels the create operation.</param>
    /// <returns>A result that succeeds when the first administrator is created.</returns>
    Task<Result> CreateInitialAdminUserAsync(
        string email,
        string password,
        string? displayName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates administrator credentials and returns a safe authenticated identity.
    /// </summary>
    /// <param name="email">The email address supplied by the login request.</param>
    /// <param name="password">The raw password supplied by the login request.</param>
    /// <param name="cancellationToken">A token that cancels the validation operation.</param>
    /// <returns>A result containing a safe administrator identity when credentials are valid.</returns>
    Task<Result<AdminUserAuthenticationDto>> ValidateAdminCredentialsAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts administrator accounts owned by the Users module.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the count operation.</param>
    /// <returns>A result containing the number of administrator accounts.</returns>
    Task<Result<int>> GetAdminUserCountAsync(CancellationToken cancellationToken = default);
}
