using Veritas.UserService.Domain.Entities;

namespace Veritas.UserService.Application.DataTransferObjects;

/// <summary>
/// Represents a password-validated administrator login challenge awaiting MFA completion.
/// </summary>
/// <param name="ChallengeId">The durable challenge identifier.</param>
/// <param name="ChallengeToken">The raw challenge token returned only once to the client.</param>
/// <param name="Purpose">The second-factor step required to complete login.</param>
/// <param name="ExpiresAtUtc">The UTC timestamp when the challenge expires.</param>
/// <param name="TotpSecretBase32">The Base32 TOTP secret shown only during first-login MFA enrollment.</param>
/// <param name="TotpProvisioningUri">The otpauth URI shown only during first-login MFA enrollment.</param>
public sealed record AdminLoginChallengeDto(
    Guid ChallengeId,
    string ChallengeToken,
    AdminLoginChallengePurpose Purpose,
    DateTime ExpiresAtUtc,
    string? TotpSecretBase32,
    string? TotpProvisioningUri);
