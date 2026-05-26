namespace Veritas.Contracts.Messages.Messaging;

/// <summary>
/// Requests asynchronous delivery of an email message.
/// </summary>
/// <param name="MessageId">The unique message identifier.</param>
/// <param name="CorrelationId">The correlation identifier that ties this message to a request or workflow.</param>
/// <param name="CausationId">The message or operation identifier that caused this request, when known.</param>
/// <param name="OccurredAtUtc">The UTC instant when the request was created.</param>
/// <param name="ActorId">The actor that caused the email request, when relevant.</param>
/// <param name="ToEmail">The destination email address.</param>
/// <param name="TemplateKey">The logical template key to render.</param>
/// <param name="TemplateModelJson">A JSON object string containing non-secret template data.</param>
public sealed record SendEmailRequestedV1(
    Guid MessageId,
    Guid CorrelationId,
    Guid? CausationId,
    DateTime OccurredAtUtc,
    Guid? ActorId,
    string ToEmail,
    string TemplateKey,
    string TemplateModelJson);
