using Ecommerce.Pedido.Api.Mensageria.Configuration;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Runtime;
using System.Text;
using System.Text.Json;

namespace Ecommerce.Pedido.Api.Mensageria.Services;

public class RabbitMqEventProcessor : IEventProcessor
{
    public readonly RabbitMqSettings _settings;

    public RabbitMqEventProcessor(IOptions<RabbitMqSettings> options)
    {
        _settings = options.Value;
    }

    public async Task PublicarEventoAsync<T>(T evento, string queueName, CancellationToken cancellationToken = default) where T : class
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.Host,
            Port = _settings.Port,
            UserName = _settings.Username,
            Password = _settings.Password
        };

        using var connection = await factory.CreateConnectionAsync(cancellationToken);
        using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        // Garante que a fila existe antes de publicar
        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: cancellationToken
        );

        // Serializa o objeto do evento para JSON
        var jsonMessage = JsonSerializer.Serialize(evento);
        var body = Encoding.UTF8.GetBytes(jsonMessage);

        // Publica a mensagem na fila informada
        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: queueName,
            mandatory: false,
            body: body,
            cancellationToken: cancellationToken
        );
    }
}