namespace Ecommerce.Pagamento.Worker.RabbitMq;

using RabbitMQ.Client;

public static class RabbitMqTopologyBuilder
{
    /// <summary>
    /// Cria uma fila principal e automaticamente configura uma DLX e DLQ para mensagens que falham.
    /// </summary>
    public static async Task DeclareQueueWithDlqAsync(
        IChannel channel,
        string queueName,
        CancellationToken cancellationToken = default)
    {
        var dlqName = $"{queueName}-dlq"; // Ex: pedido-criado-queue-dlq
        var dlxName = $"{queueName}-dlx"; // Ex: pedido-criado-queue-dlx

        // 1. Declara a Exchange de Letras Mortas (DLX)
        await channel.ExchangeDeclareAsync(
            exchange: dlxName,
            type: ExchangeType.Direct,
            durable: true,
            cancellationToken: cancellationToken);

        // 2. Declara a Fila de Letras Mortas (DLQ)
        await channel.QueueDeclareAsync(
            queue: dlqName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken);

        // 3. Faz o Bind (Liga a DLQ à DLX)
        await channel.QueueBindAsync(
            queue: dlqName,
            exchange: dlxName,
            routingKey: dlqName,
            cancellationToken: cancellationToken);

        // 4. Declara a Fila Principal configurada para reencaminhar falhas para a DLX
        var args = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", dlxName },
            { "x-dead-letter-routing-key", dlqName }
        };

        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: args,
            cancellationToken: cancellationToken);
    }
}