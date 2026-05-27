namespace Veritas.Admin.API.Models.AdminAuth;

public sealed class AdminLoginRequest
{
    /// <summary>
    /// Gets or sets the administrator email address supplied for login.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Gets or sets the raw administrator password supplied for login.
    /// </summary>
    public required string Password { get; set; }
}
