using Veritas.Contracts.Messages.Messaging;
using Veritas.MessagingService.Application.Services;

namespace Veritas.Admin.API.Modules.Messaging;

/// <summary>
/// Handles RabbitMQ-backed templated email delivery requests.
/// </summary>
public sealed class SendTemplatedEmailRequestedHandler(
    ILogger<SendTemplatedEmailRequestedHandler> logger,
    EmailDeliveryService emailDeliveryService)
{
    /// <summary>
    /// Renders and delivers a templated email request.
    /// </summary>
    /// <param name="message">The durable templated email request.</param>
    /// <param name="cancellationToken">A token that cancels delivery.</param>
    /// <exception cref="InvalidOperationException">Thrown when delivery fails so Wolverine can retry or dead-letter.</exception>
    public async Task Handle(SendTemplatedEmailRequestedV1 message, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Received templated email request {MessageId} for template {TemplateKey}.",
            message.MessageId,
            message.TemplateKey);

        var result = await emailDeliveryService.SendTemplatedEmailAsync(
            message.TenantId,
            message.ToEmail,
            message.TemplateKey,
            message.TemplateModelJson,
            cancellationToken);

        if (result.IsFailed)
        {
            var errorCode = result.Errors.FirstOrDefault()?.Metadata.TryGetValue("Code", out var code) == true
                ? code?.ToString()
                : "MESSAGING_DELIVERY_FAILED";

            logger.LogWarning(
                "Templated email request {MessageId} failed with error code {ErrorCode}.",
                message.MessageId,
                errorCode);
            throw new InvalidOperationException($"Templated email delivery failed with error code {errorCode}.");
        }
    }
}
