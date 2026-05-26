using FluentResults;

namespace Veritas.PlatformService.Application.Interfaces;

public interface IBootstrapService
{
    /// <summary>
    /// Determines whether the platform still requires bootstrap.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the lookup.</param>
    /// <returns>A result containing <see langword="true" /> when bootstrap is still required.</returns>
    Task<Result<bool>> GetBootstrapStatus(CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts a bootstrap session for the first admin user.
    /// </summary>
    /// <param name="email">The requested first admin email address.</param>
    /// <param name="bootstrapSecret">The setup secret supplied by the caller.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A result containing the bootstrap session token when a session starts.</returns>
    Task<Result<string>> StartBootstrap(
        string email,
        string bootstrapSecret,
        CancellationToken cancellationToken = default);
}
