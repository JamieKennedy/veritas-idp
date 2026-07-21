namespace Veritas.Admin.API.Models.AdminAuth;

/// <summary>
/// Represents a completed administrator MFA enrollment response.
/// </summary>
/// <param name="Admin">The authenticated administrator identity.</param>
/// <param name="RecoveryCodes">The one-time recovery codes returned only once after enrollment.</param>
public sealed record AdminMfaEnrollmentResponse(AdminLoginResponse Admin, IReadOnlyList<string> RecoveryCodes);
