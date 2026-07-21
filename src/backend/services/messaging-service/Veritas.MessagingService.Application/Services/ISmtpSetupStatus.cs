namespace Veritas.MessagingService.Application.Services;

/// <summary>
/// Reads whether SMTP setup has completed successfully.
/// </summary>
public interface ISmtpSetupStatus
{
    /// <summary>
    /// Indicates whether SMTP has passed setup and can be used by protected admin workflows.
    /// </summary>
    /// <param name="cancellationToken">A token that cancels the query.</param>
    /// <returns>True when SMTP is configured.</returns>
    Task<bool> IsSmtpConfiguredAsync(CancellationToken cancellationToken = default);
}
