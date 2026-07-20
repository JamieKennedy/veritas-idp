namespace Veritas.UserService.Domain.Entities;

/// <summary>
/// Describes which second-factor step a password-validated administrator login challenge is waiting for.
/// </summary>
public enum AdminLoginChallengePurpose
{
    /// <summary>
    /// The administrator must confirm a newly generated TOTP secret before an admin session is issued.
    /// </summary>
    MfaEnrollment = 1,

    /// <summary>
    /// The administrator must prove possession of an already-enrolled MFA factor before an admin session is issued.
    /// </summary>
    MfaVerification = 2
}
