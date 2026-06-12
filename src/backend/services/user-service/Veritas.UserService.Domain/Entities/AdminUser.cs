namespace Veritas.UserService.Domain.Entities;

public class AdminUser
{
    /// <summary>
    /// Gets the durable administrator identifier.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets or sets the normalized administrator email address used for login and uniqueness checks.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Gets or sets the versioned password hash. Raw passwords must never be stored here.
    /// </summary>
    public required string PasswordHash { get; set; } 

    /// <summary>
    /// Gets or sets the optional administrator display name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the singleton bootstrap slot for the first administrator account.
    /// This is <see langword="null" /> for administrator accounts created after bootstrap.
    /// </summary>
    public int? InitialAdminSlot { get; set; }

    /// <summary>
    /// Gets or sets the protected TOTP shared secret for administrator MFA.
    /// The plaintext secret must never be stored in this property.
    /// </summary>
    public string? MfaSecretProtected { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when administrator MFA was enabled.
    /// A <see langword="null" /> value means the administrator must enroll MFA before login completes.
    /// </summary>
    public DateTime? MfaEnabledAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when administrator MFA configuration last changed.
    /// </summary>
    public DateTime? MfaUpdatedAtUtc { get; set; }

    /// <summary>
    /// Gets or sets the stamp copied into admin sessions so account security changes can revoke old cookies.
    /// </summary>
    public Guid SecurityStamp { get; set; }

    /// <summary>
    /// Gets the UTC timestamp when the administrator account was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>
    /// Gets the UTC timestamp when the administrator account was last updated.
    /// </summary>
    public DateTime UpdatedAtUtc { get; init; }
}
