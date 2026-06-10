using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using FluentResults;
using Microsoft.Extensions.Logging;
using Veritas.MessagingService.Application.Services;
using Veritas.MessagingService.Domain.Errors;
using Veritas.MessagingService.Domain.Types;

namespace Veritas.MessagingService.Infrastructure.Email;

/// <summary>
/// Sends rendered email through SMTP.
/// </summary>
public sealed class SmtpEmailSender(ILogger<SmtpEmailSender> logger) : IEmailSender
{
    /// <inheritdoc />
    public async Task<Result> SendAsync(EmailSendRequest request, CancellationToken cancellationToken)
    {
        try
        {
            using var message = new MailMessage
            {
                From = string.IsNullOrWhiteSpace(request.FromName)
                    ? new MailAddress(request.FromEmail)
                    : new MailAddress(request.FromEmail, request.FromName),
                Subject = request.Subject,
                Body = request.TextBody,
                IsBodyHtml = false
            };
            message.To.Add(request.ToEmail);
            message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(
                request.HtmlBody,
                Encoding.UTF8,
                MediaTypeNames.Text.Html));

            using var client = new SmtpClient(request.SmtpHost, request.SmtpPort)
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
                "SMTP delivery failed for host {SmtpHost} and port {SmtpPort}.",
                request.SmtpHost,
                request.SmtpPort);
            return Result.Fail(new SmtpConnectivityFailedError());
        }
    }
}
