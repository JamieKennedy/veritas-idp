namespace Veritas.Admin.API.Models.Setup;

public sealed class StartBootstrapRequest
{
    /// <summary>
    /// Gets or sets the email address to use for the first administrator account.
    /// </summary>
    public required string Email
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the deployment bootstrap secret supplied by the operator.
    /// </summary>
    public required string BootstrapSecret
    {
        get; set;
    }
}
