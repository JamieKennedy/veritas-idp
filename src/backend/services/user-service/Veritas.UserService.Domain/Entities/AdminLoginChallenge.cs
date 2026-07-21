namespace Veritas.UserService.Domain.Entities;

/// <summary>
/// Stores a short-lived password-validated administrator login challenge.
/// </summary>
public class AdminLoginChallenge
{
    /// <summary>
    /// Gets the durable login challenge identifier.
    /// </summary>
    public Guid Id
    {
        get; init;
    }

    /// <summary>
    /// Gets or sets the administrator account that owns the challenge.
    /// </summary>
    public Guid AdminUserId
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the second-factor step required to complete the login.
    /// </summary>
    public AdminLoginChallengePurpose Purpose
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the salted hash of the raw challenge token sent once to the client.
    /// </summary>
    public required string ChallengeTokenHash
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the protected pending TOTP secret used during first-login MFA enrollment.
    /// </summary>
    public string? PendingMfaSecretProtected
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the UTC timestamp when the challenge was created.
    /// </summary>
    public DateTime CreatedAtUtc
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the UTC timestamp when the challenge expires.
    /// </summary>
    public DateTime ExpiresAtUtc
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the UTC timestamp when the challenge was consumed.
    /// A <see langword="null" /> value means the challenge is still unused.
    /// </summary>
    public DateTime? ConsumedAtUtc
    {
        get; set;
    }
}
