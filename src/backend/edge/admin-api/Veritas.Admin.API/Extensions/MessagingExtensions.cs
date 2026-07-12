using Veritas.Contracts.Messages.Messaging;

using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Postgresql;
using Wolverine.RabbitMQ;

namespace Veritas.Admin.API.Extensions;

public static class MessagingExtensions
{
    /// <summary>
    /// Configures Wolverine, RabbitMQ transport, and PostgreSQL-backed durable message storage.
    /// </summary>
    /// <param name="builder">The web application builder that owns host configuration.</param>
    public static void ConfigureVeritasMessaging(this WebApplicationBuilder builder)
    {
        builder.Host.UseWolverine(options =>
        {
            var connectionString = builder.Configuration.GetConnectionString("VeritasDb");

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                options.PersistMessagesWithPostgresql(connectionString);
            }

            options.UseRabbitMqUsingNamedConnection("messaging")
                .DisableSystemRequestReplyQueueDeclaration()
                .AutoProvision();
            options.PublishMessage<SendTemplatedEmailRequestedV1>().ToRabbitExchange("veritas.messaging.email");
            options.ListenToRabbitQueue("veritas.messaging.email")
                .DefaultIncomingMessage<SendTemplatedEmailRequestedV1>();
            options.UseEntityFrameworkCoreTransactions();
        });
    }
}
