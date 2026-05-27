using Veritas.Shared.Errors;

namespace Veritas.UserService.Domain.Errors.AdminUsers;

/// <summary>
/// Error returned when an administrator email address is missing or malformed.
/// </summary>
public sealed class InvalidAdminEmailError : ValidationError
{
    public const string ErrorCode = "USERS_ADMIN_EMAIL_INVALID";

    public InvalidAdminEmailError() : base(ErrorCode, "A valid administrator email address is required.")
    {
    }
}
