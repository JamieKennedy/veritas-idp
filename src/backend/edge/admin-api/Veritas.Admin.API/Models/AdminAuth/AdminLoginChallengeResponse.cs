using Veritas.UserService.Domain.Entities;

namespace Veritas.Admin.API.Models.AdminAuth;

/// <summary>
/// Represents the MFA step required after administrator password validation.
/// </summary>
/// <param name="ChallengeId">The durable login challenge identifier.</param>
/// <param name="ChallengeToken">The raw challenge token required to complete MFA.</param>
/// <param name="Purpose">The MFA purpose required to complete the login.</param>
/// <param name="ExpiresAtUtc">The UTC timestamp when the challenge expires.</param>
/// <param name="TotpSecretBase32">The Base32 TOTP secret shown only during enrollment.</param>
/// <param name="TotpProvisioningUri">The otpauth URI shown only during enrollment.</param>
public sealed record AdminLoginChallengeResponse(
    Guid ChallengeId,
    string ChallengeToken,
    AdminLoginChallengePurpose Purpose,
    DateTime ExpiresAtUtc,
    string? TotpSecretBase32,
    string? TotpProvisioningUri);
