using Veritas.Shared.Errors;

namespace Veritas.UserService.Domain.Errors.AdminUsers;

/// <summary>
/// Error returned when the first administrator account could not be persisted.
/// </summary>
public sealed class InitialAdminCreationFailedError : UnexpectedError
{
    public const string ErrorCode = "USERS_INITIAL_ADMIN_CREATE_FAILED";

    public InitialAdminCreationFailedError() : base(ErrorCode, "Failed to create initial administrator account.")
    {
    }
}
