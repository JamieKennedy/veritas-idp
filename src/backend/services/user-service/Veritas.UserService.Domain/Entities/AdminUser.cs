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
    /// Gets the UTC timestamp when the administrator account was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; init; }

    /// <summary>
    /// Gets the UTC timestamp when the administrator account was last updated.
    /// </summary>
    public DateTime UpdatedAtUtc { get; init; }
}
