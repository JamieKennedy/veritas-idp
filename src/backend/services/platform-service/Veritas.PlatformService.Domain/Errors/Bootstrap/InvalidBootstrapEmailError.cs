using Veritas.Shared.Errors;

namespace Veritas.PlatformService.Domain.Errors.Bootstrap;

public sealed class InvalidBootstrapEmailError : ValidationError
{
    public const string ErrorCode = "PLATFORM_BOOTSTRAP_EMAIL_INVALID";

    public InvalidBootstrapEmailError() : base(ErrorCode, "A valid first administrator email address is required.")
    {
    }
}
