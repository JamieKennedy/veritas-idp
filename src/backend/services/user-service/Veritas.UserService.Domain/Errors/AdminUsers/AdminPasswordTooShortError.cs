using Veritas.Shared.Errors;

namespace Veritas.UserService.Domain.Errors.AdminUsers;

/// <summary>
/// Error returned when an administrator password does not meet the minimum length.
/// </summary>
public sealed class AdminPasswordTooShortError : ValidationError
{
    public const string ErrorCode = "USERS_ADMIN_PASSWORD_TOO_SHORT";

    public AdminPasswordTooShortError(int minimumLength)
        : base(ErrorCode, $"The administrator password must be at least {minimumLength} characters long.")
    {
    }
}
