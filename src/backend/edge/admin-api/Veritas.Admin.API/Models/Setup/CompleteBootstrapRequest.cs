namespace Veritas.Admin.API.Models.Setup;

public sealed class CompleteBootstrapRequest
{
    /// <summary>
    /// Gets or sets the raw password for the first administrator account.
    /// </summary>
    public required string Password
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the optional display name for the first administrator account.
    /// </summary>
    public string? DisplayName
    {
        get; set;
    }
}
