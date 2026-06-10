using FluentResults;

namespace Veritas.MessagingService.Application.Services;

/// <summary>
/// Sends rendered email through the configured adapter.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends a rendered email message.
    /// </summary>
    /// <param name="request">The SMTP delivery request.</param>
    /// <param name="cancellationToken">A token that cancels delivery.</param>
    /// <returns>A success result when the adapter accepts the email.</returns>
    Task<Result> SendAsync(EmailSendRequest request, CancellationToken cancellationToken);
}
