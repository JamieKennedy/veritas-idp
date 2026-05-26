namespace Veritas.PlatformService.Domain.Types;

public enum EBootstrapSessionStatus
{
    PendingVerification,
    Verified,
    Completed,
    Expired,
    Cancelled
}