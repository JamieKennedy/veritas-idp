using FluentResults;

namespace Veritas.MessagingService.Application.Services;

/// <summary>
/// Sends a live SMTP test message before settings are persisted.
/// </summary>
public interface ISmtpConnectivityTester
{
    /// <summary>
    /// Tests whether SMTP settings can send email.
    /// </summary>
    /// <param name="request">The SMTP settings to test.</param>
    /// <param name="cancellationToken">A token that cancels the test.</param>
    /// <returns>A success result when the SMTP relay accepts the test message.</returns>
    Task<Result> TestAsync(SmtpConnectivityTestRequest request, CancellationToken cancellationToken);
}
