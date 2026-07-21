using Veritas.Shared.Errors;

namespace Veritas.UserService.Domain.Errors.AdminUsers;

/// <summary>
/// Error returned when administrator credential validation cannot be completed.
/// </summary>
public sealed class AdminCredentialValidationFailedError : UnexpectedError
{
    public const string ErrorCode = "USERS_ADMIN_CREDENTIAL_VALIDATION_FAILED";

    public AdminCredentialValidationFailedError() : base(ErrorCode, "Failed to validate administrator credentials.")
    {
    }
}
