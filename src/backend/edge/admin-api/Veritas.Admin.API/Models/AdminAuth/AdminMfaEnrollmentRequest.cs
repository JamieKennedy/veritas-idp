namespace Veritas.Admin.API.Models.AdminAuth;

/// <summary>
/// Represents an administrator request to confirm first-login TOTP enrollment.
/// </summary>
/// <param name="ChallengeId">The login challenge identifier returned by password validation.</param>
/// <param name="ChallengeToken">The raw challenge token returned by password validation.</param>
/// <param name="TotpCode">The TOTP code generated from the pending enrollment secret.</param>
public sealed record AdminMfaEnrollmentRequest(Guid ChallengeId, string ChallengeToken, string TotpCode);
