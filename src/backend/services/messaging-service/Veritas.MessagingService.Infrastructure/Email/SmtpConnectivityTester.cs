using System.Net;
using System.Net.Mail;
using FluentResults;
using Microsoft.Extensions.Logging;
using Veritas.MessagingService.Application.Services;
using Veritas.MessagingService.Domain.Errors;
using Veritas.MessagingService.Domain.Types;

namespace Veritas.MessagingService.Infrastructure.Email;

/// <summary>
/// Performs a real SMTP test send before settings are persisted.
/// </summary>
public sealed class SmtpConnectivityTester(ILogger<SmtpConnectivityTester> logger) : ISmtpConnectivityTester
{
    /// <inheritdoc />
    public async Task<Result> TestAsync(SmtpConnectivityTestRequest request, CancellationToken cancellationToken)
    {
        try
        {
            using var message = new MailMessage
            {
                From = string.IsNullOrWhiteSpace(request.FromName)
                    ? new MailAddress(request.FromEmail)
                    : new MailAddress(request.FromEmail, request.FromName),
                Subject = "Veritas SMTP setup test",
                Body = "Veritas SMTP setup test.",
                IsBodyHtml = false
            };
            message.To.Add(request.FromEmail);

            using var client = new SmtpClient(request.Host, request.Port)
            {
                EnableSsl = request.TlsMode == SmtpTlsMode.StartTls,
                DeliveryMethod = SmtpDeliveryMethod.Network
            };

            if (!string.IsNullOrWhiteSpace(request.Username))
            {
                client.Credentials = new NetworkCredential(request.Username, request.Secret ?? string.Empty);
            }

            await client.SendMailAsync(message, cancellationToken);
            return Result.Ok();
        }
        catch (Exception exception) when (exception is SmtpException or InvalidOperationException)
        {
            logger.LogWarning(
                exception,
                "SMTP connectivity test failed for host {SmtpHost} and port {SmtpPort}.",
                request.Host,
                request.Port);
            return Result.Fail(new SmtpConnectivityFailedError());
        }
    }
}
