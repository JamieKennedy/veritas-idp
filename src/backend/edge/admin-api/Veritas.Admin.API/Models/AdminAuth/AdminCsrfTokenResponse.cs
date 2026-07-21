namespace Veritas.Admin.API.Models.AdminAuth;

/// <summary>
/// Represents an antiforgery request token for cookie-authenticated Admin API calls.
/// </summary>
/// <param name="Token">The request token that clients must send in the X-CSRF-TOKEN header.</param>
public sealed record AdminCsrfTokenResponse(string Token);
