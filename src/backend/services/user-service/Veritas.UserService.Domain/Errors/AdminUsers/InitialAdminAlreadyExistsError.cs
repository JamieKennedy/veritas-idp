using Veritas.Shared.Errors;

namespace Veritas.UserService.Domain.Errors.AdminUsers;

/// <summary>
/// Error returned when a first administrator already exists.
/// </summary>
public sealed class InitialAdminAlreadyExistsError : ConflictError
{
    public const string ErrorCode = "USERS_INITIAL_ADMIN_ALREADY_EXISTS";

    public InitialAdminAlreadyExistsError() : base(ErrorCode, "The first administrator account has already been created.")
    {
    }
}
