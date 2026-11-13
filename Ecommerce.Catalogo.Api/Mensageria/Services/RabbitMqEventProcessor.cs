// Substitua o using do Pedido pelo do Catálogo:
using Ecommerce.Catalogo.Api.Mensageria.Settings
    ;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Ecommerce.Catalogo.Api.Mensageria.Services;

public class RabbitMqEventProcessor : IEventProcessor
{
    private readonly RabbitMqSettings _settings;

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

        // 🎯 APAGÁMOS o QueueDeclareAsync antigo e COLOCÁMOS o Builder com a DLQ:
        await RabbitMqTopologyBuilder.DeclareQueueWithDlqAsync(channel, queueName, cancellationToken);

        var jsonMessage = JsonSerializer.Serialize(evento);
        var body = Encoding.UTF8.GetBytes(jsonMessage);

        await channel.BasicPublishAsync(
            exchange: string.Empty,
            routingKey: queueName,
            mandatory: false,
            body: body,
            cancellationToken: cancellationToken
        );
    }
}