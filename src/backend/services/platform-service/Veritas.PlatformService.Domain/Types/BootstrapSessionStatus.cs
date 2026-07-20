namespace Veritas.PlatformService.Domain.Types;

public enum BootstrapSessionStatus
{
    PendingVerification,
    Verified,
    Completed,
    Expired,
    Cancelled
}
