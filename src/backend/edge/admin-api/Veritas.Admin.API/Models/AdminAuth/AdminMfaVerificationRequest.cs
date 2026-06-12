namespace Veritas.Admin.API.Models.AdminAuth;

/// <summary>
/// Represents an administrator request to complete MFA verification.
/// </summary>
/// <param name="ChallengeId">The login challenge identifier returned by password validation.</param>
/// <param name="ChallengeToken">The raw challenge token returned by password validation.</param>
/// <param name="Code">The TOTP or recovery code supplied by the administrator.</param>
public sealed record AdminMfaVerificationRequest(Guid ChallengeId, string ChallengeToken, string Code);
