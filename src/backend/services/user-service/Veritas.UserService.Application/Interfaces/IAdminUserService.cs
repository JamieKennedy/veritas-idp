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
    /// Validates administrator credentials and creates a short-lived MFA login challenge.
    /// </summary>
    /// <param name="email">The email address supplied by the login request.</param>
    /// <param name="password">The raw password supplied by the login request.</param>
    /// <param name="cancellationToken">A token that cancels the login start operation.</param>
    /// <returns>A result containing the MFA challenge required before an admin session is issued.</returns>
    Task<Result<AdminLoginChallengeDto>> StartAdminLoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes first-login administrator MFA enrollment and issues an admin session.
    /// </summary>
    /// <param name="challengeId">The MFA enrollment challenge identifier.</param>
    /// <param name="challengeToken">The raw challenge token returned when the challenge was created.</param>
    /// <param name="totpCode">The TOTP code generated from the pending enrollment secret.</param>
    /// <param name="cancellationToken">A token that cancels the enrollment operation.</param>
    /// <returns>A result containing the authenticated admin session and one-time recovery codes.</returns>
    Task<Result<AdminMfaEnrollmentResultDto>> CompleteAdminMfaEnrollmentAsync(
        Guid challengeId,
        string challengeToken,
        string totpCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes administrator MFA verification and issues an admin session.
    /// </summary>
    /// <param name="challengeId">The MFA verification challenge identifier.</param>
    /// <param name="challengeToken">The raw challenge token returned when the challenge was created.</param>
    /// <param name="code">The TOTP or recovery code supplied by the administrator.</param>
    /// <param name="cancellationToken">A token that cancels the verification operation.</param>
    /// <returns>A result containing the authenticated admin session.</returns>
    Task<Result<AdminSessionAuthenticationDto>> CompleteAdminMfaVerificationAsync(
        Guid challengeId,
        string challengeToken,
        string code,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates an existing server-side administrator session.
    /// </summary>
    /// <param name="adminUserId">The administrator identifier from the auth cookie.</param>
    /// <param name="sessionId">The server-side session identifier from the auth cookie.</param>
    /// <param name="securityStamp">The security stamp from the auth cookie.</param>
    /// <param name="cancellationToken">A token that cancels the validation operation.</param>
    /// <returns>A result containing the refreshed session when it is valid.</returns>
    Task<Result<AdminSessionAuthenticationDto>> ValidateAdminSessionAsync(
        Guid adminUserId,
        Guid sessionId,
        Guid securityStamp,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes an existing server-side administrator session.
    /// </summary>
    /// <param name="adminUserId">The administrator identifier that owns the session.</param>
    /// <param name="sessionId">The session identifier to revoke.</param>
    /// <param name="reason">The safe operational reason for revocation.</param>
    /// <param name="cancellationToken">A token that cancels the revocation operation.</param>
    /// <returns>A result that succeeds when the session is revoked or already absent.</returns>
    Task<Result> RevokeAdminSessionAsync(
        Guid adminUserId,
        Guid sessionId,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts administrator accounts owned by the Users module.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the count operation.</param>
    /// <returns>A result containing the number of administrator accounts.</returns>
    Task<Result<int>> GetAdminUserCountAsync(CancellationToken cancellationToken = default);
}
