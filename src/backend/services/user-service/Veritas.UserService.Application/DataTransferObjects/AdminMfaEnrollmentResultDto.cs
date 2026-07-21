namespace Veritas.UserService.Application.DataTransferObjects;

/// <summary>
/// Represents a completed administrator MFA enrollment and newly issued admin session.
/// </summary>
/// <param name="Admin">The safe administrator identity.</param>
/// <param name="SessionId">The server-side session identifier copied into the auth cookie.</param>
/// <param name="SecurityStamp">The security stamp copied into the auth cookie for revocation checks.</param>
/// <param name="IdleExpiresAtUtc">The UTC timestamp when the session expires if it is not used.</param>
/// <param name="AbsoluteExpiresAtUtc">The UTC timestamp when the session expires regardless of activity.</param>
/// <param name="RecoveryCodes">The raw one-time recovery codes returned only once after enrollment.</param>
public sealed record AdminMfaEnrollmentResultDto(
    AdminUserAuthenticationDto Admin,
    Guid SessionId,
    Guid SecurityStamp,
    DateTime IdleExpiresAtUtc,
    DateTime AbsoluteExpiresAtUtc,
    IReadOnlyList<string> RecoveryCodes) : AdminSessionAuthenticationDto(
        Admin,
        SessionId,
        SecurityStamp,
        IdleExpiresAtUtc,
        AbsoluteExpiresAtUtc);
