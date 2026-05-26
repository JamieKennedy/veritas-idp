using Veritas.Contracts.Messages.Messaging;

namespace Veritas.Admin.API.Modules.Messaging;

public sealed class SendEmailRequestedHandler(ILogger<SendEmailRequestedHandler> logger)
{
    /// <summary>
    /// Handles an asynchronous email-delivery request.
    /// </summary>
    /// <param name="message">The email request message to handle.</param>
    /// <param name="cancellationToken">A token that cancels email handling.</param>
    public Task Handle(SendEmailRequestedV1 message, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Received email request {MessageId} for template {TemplateKey}.",
            message.MessageId,
            message.TemplateKey);

        return Task.CompletedTask;
    }
}
