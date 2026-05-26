using Veritas.PlatformService.Domain.Types;

namespace Veritas.PlatformService.Domain.Entities;

public class BootstrapSession
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public EBootstrapSessionStatus Status { get; set; } // move to enum
    public string BootstrapSecretHash { get; set; }
    public string OtpHash  { get; set; }
    public string SessionTokenHash { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime VerifiedAtUtc { get; set; }
    public DateTime CompletedAtUtc { get; set; }
    public int AttemptCount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}