namespace Veritas.UserService.Domain.Entities;

/// <summary>
/// Stores a server-side administrator session that backs a browser authentication cookie.
/// </summary>
public class AdminSession
{
    /// <summary>
    /// Gets the durable session identifier copied into the admin auth cookie.
    /// </summary>
    public Guid Id
    {
        get; init;
    }

    /// <summary>
    /// Gets or sets the administrator account that owns the session.
    /// </summary>
    public Guid AdminUserId
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the admin security stamp that was current when the session was issued.
    /// </summary>
    public Guid SecurityStamp
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the UTC timestamp when the session was issued.
    /// </summary>
    public DateTime IssuedAtUtc
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the UTC timestamp when the session was last validated.
    /// </summary>
    public DateTime LastSeenAtUtc
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the UTC idle expiry timestamp for the session.
    /// </summary>
    public DateTime IdleExpiresAtUtc
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the UTC absolute expiry timestamp for the session.
    /// </summary>
    public DateTime AbsoluteExpiresAtUtc
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the UTC timestamp when the session was revoked.
    /// A <see langword="null" /> value means the session is still active.
    /// </summary>
    public DateTime? RevokedAtUtc
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the safe operational reason recorded when the session is revoked.
    /// </summary>
    public string? RevocationReason
    {
        get; set;
    }
}
