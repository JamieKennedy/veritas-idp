namespace Veritas.UserService.Domain.Entities;

/// <summary>
/// Stores a hashed one-time administrator MFA recovery code.
/// </summary>
public class AdminRecoveryCode
{
    /// <summary>
    /// Gets the durable recovery code record identifier.
    /// </summary>
    public Guid Id
    {
        get; init;
    }

    /// <summary>
    /// Gets or sets the administrator account that owns the recovery code.
    /// </summary>
    public Guid AdminUserId
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the salted recovery code hash. Raw recovery codes must never be stored here.
    /// </summary>
    public required string CodeHash
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the UTC timestamp when the recovery code was created.
    /// </summary>
    public DateTime CreatedAtUtc
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the UTC timestamp when the recovery code was consumed.
    /// A <see langword="null" /> value means the code has not been used.
    /// </summary>
    public DateTime? ConsumedAtUtc
    {
        get; set;
    }
}
