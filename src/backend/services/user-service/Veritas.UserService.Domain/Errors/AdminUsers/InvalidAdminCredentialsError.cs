using Veritas.Shared.Errors;

namespace Veritas.UserService.Domain.Errors.AdminUsers;

/// <summary>
/// Error returned when administrator credentials are invalid.
/// </summary>
public sealed class InvalidAdminCredentialsError : UnauthorizedError
{
    public const string ErrorCode = "USERS_INVALID_ADMIN_CREDENTIALS";

    public InvalidAdminCredentialsError() : base(ErrorCode, "Invalid administrator credentials.")
    {
    }
}
