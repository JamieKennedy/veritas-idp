using FluentResults;

using Veritas.PlatformService.Application.DataTransferObjects.Setup;

namespace Veritas.PlatformService.Application.Interfaces;

public interface IBootstrapService
{
    /// <summary>
    /// Gets the current platform bootstrap state.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the lookup.</param>
    /// <returns>A result containing the current bootstrap state.</returns>
    Task<Result<BootstrapStatusDto>> GetBootstrapStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts a bootstrap session for the first admin user.
    /// </summary>
    /// <param name="email">The requested first admin email address.</param>
    /// <param name="bootstrapSecret">The setup secret supplied by the caller.</param>
    /// <param name="createdFromIp">The remote IP address that started the bootstrap session, when available.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A result containing the bootstrap session token when a session starts.</returns>
    Task<Result<string>> StartBootstrapAsync(
        string email,
        string bootstrapSecret,
        string? createdFromIp,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes bootstrap by creating the first administrator from the active bootstrap session.
    /// </summary>
    /// <param name="sessionToken">The raw bootstrap session token from the secure bootstrap cookie.</param>
    /// <param name="password">The raw password for the first administrator account.</param>
    /// <param name="displayName">The optional first administrator display name.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A result that succeeds when bootstrap completion is durable.</returns>
    Task<Result> CompleteBootstrapAsync(
        string sessionToken,
        string password,
        string? displayName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records that an administrator has intentionally deferred SMTP setup.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>A result that succeeds when SMTP deferral is durable.</returns>
    Task<Result> DeferSmtpSetupAsync(CancellationToken cancellationToken = default);
}
