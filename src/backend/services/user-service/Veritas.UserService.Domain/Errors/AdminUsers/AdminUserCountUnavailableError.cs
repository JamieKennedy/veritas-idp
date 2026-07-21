using Veritas.Shared.Errors;

namespace Veritas.UserService.Domain.Errors.AdminUsers;

/// <summary>
/// Error returned when the Users module cannot read administrator account count.
/// </summary>
public sealed class AdminUserCountUnavailableError : UnexpectedError
{
    public const string ErrorCode = "USERS_ADMIN_COUNT_UNAVAILABLE";

    public AdminUserCountUnavailableError() : base(ErrorCode, "Failed to get admin user count.")
    {
    }
}
