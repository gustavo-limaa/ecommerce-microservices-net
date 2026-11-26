using RabbitMQ.Client;

namespace Ecommerce.Catalogo.Api.Mensageria.Services;

public static class RabbitMqTopologyBuilder
{
    public static async Task DeclareQueueWithDlqAsync(
        IChannel channel,
        string queueName,
        CancellationToken cancellationToken = default)
    {
        var dlqName = $"{queueName}-dlq";
        var dlxName = $"{queueName}-dlx";

        await channel.ExchangeDeclareAsync(exchange: dlxName, type: ExchangeType.Direct, durable: true, cancellationToken: cancellationToken);
        await channel.QueueDeclareAsync(queue: dlqName, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: cancellationToken);
        await channel.QueueBindAsync(queue: dlqName, exchange: dlxName, routingKey: dlqName, cancellationToken: cancellationToken);

        var args = new Dictionary<string, object>
    {
        { "x-dead-letter-exchange", dlxName },
        { "x-dead-letter-routing-key", dlqName }
    };

        await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: args, cancellationToken: cancellationToken);
    }
}