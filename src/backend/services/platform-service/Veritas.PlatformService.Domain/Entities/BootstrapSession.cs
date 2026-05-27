using Veritas.PlatformService.Domain.Types;

namespace Veritas.PlatformService.Domain.Entities;

public class BootstrapSession
{
    /// <summary>
    /// Gets or sets the durable bootstrap session identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the normalized email address for the initial administrator candidate.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Gets or sets the current bootstrap session lifecycle state.
    /// </summary>
    public EBootstrapSessionStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the hash of the bootstrap secret used to start the session.
    /// </summary>
    public string BootstrapSecretHash { get; set; } = null!;

    /// <summary>
    /// Gets or sets the hash of the session token used for subsequent bootstrap requests.
    /// </summary>
    public string SessionTokenHash { get; set; } = null!;

    /// <summary>
    /// Gets or sets the UTC timestamp when the bootstrap session expires.
    /// </summary>
    public DateTime ExpiresAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the bootstrap session was verified.
    /// </summary>
    public DateTime? VerifiedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when bootstrap completed.
    /// </summary>
    public DateTime? CompletedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the singleton active-bootstrap slot.
    /// This is <see langword="null" /> after the session is completed, expired, or cancelled.
    /// </summary>
    public int? ActiveBootstrapSlot { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the bootstrap session was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the remote IP address that created the bootstrap session, when available.
    /// </summary>
    public string? CreatedFromIp { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the bootstrap session was last used.
    /// </summary>
    public DateTime? LastSeenAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the bootstrap session was last updated.
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; }
}
