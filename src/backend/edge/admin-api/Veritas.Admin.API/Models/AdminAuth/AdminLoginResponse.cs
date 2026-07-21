namespace Veritas.Admin.API.Models.AdminAuth;

/// <summary>
/// Represents the authenticated administrator returned after a successful login.
/// </summary>
/// <param name="Id">The durable administrator identifier.</param>
/// <param name="Email">The normalized administrator email address.</param>
/// <param name="Name">The optional administrator display name.</param>
public sealed record AdminLoginResponse(Guid Id, string Email, string? Name);
