namespace Veritas.UserService.Application.DataTransferObjects;

/// <summary>
/// Represents a successfully authenticated administrator without exposing credential material.
/// </summary>
/// <param name="Id">The durable administrator identifier.</param>
/// <param name="Email">The normalized administrator email address.</param>
/// <param name="Name">The optional administrator display name.</param>
public sealed record AdminUserAuthenticationDto(Guid Id, string Email, string? Name);
