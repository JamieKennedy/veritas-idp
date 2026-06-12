namespace Veritas.UserService.Application.DataTransferObjects;

/// <summary>
/// Represents a fully authenticated administrator session safe for cookie claims and HTTP responses.
/// </summary>
/// <param name="Admin">The safe administrator identity.</param>
/// <param name="SessionId">The server-side session identifier copied into the auth cookie.</param>
/// <param name="SecurityStamp">The security stamp copied into the auth cookie for revocation checks.</param>
/// <param name="IdleExpiresAtUtc">The UTC timestamp when the session expires if it is not used.</param>
/// <param name="AbsoluteExpiresAtUtc">The UTC timestamp when the session expires regardless of activity.</param>
public record AdminSessionAuthenticationDto(
    AdminUserAuthenticationDto Admin,
    Guid SessionId,
    Guid SecurityStamp,
    DateTime IdleExpiresAtUtc,
    DateTime AbsoluteExpiresAtUtc);
