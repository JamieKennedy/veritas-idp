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
    /// Gets or sets the hash of the one-time password used to verify the session.
    /// </summary>
    public string OtpHash { get; set; } = null!;

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
    public DateTime VerifiedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when bootstrap completed.
    /// </summary>
    public DateTime CompletedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the number of verification attempts made for the session.
    /// </summary>
    public int AttemptCount { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the bootstrap session was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when the bootstrap session was last updated.
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; }
}
