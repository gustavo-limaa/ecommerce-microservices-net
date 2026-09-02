using Ecommerce.Pedido.Api.Infrastructure.Data;
using Ecommerce.Pedido.Api.Mensageria.Configuration;
using Ecommerce.Pedido.Api.Mensageria.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Ecommerce.Pedido.Api.Mensageria.Services;

public class PagamentoConcluidoConsumer(
   ILogger<PagamentoConcluidoConsumer> logger,
   IOptions<RabbitMqSettings> rabbitOptions,
   IServiceProvider serviceProvider) : BackgroundService
{
    private const string QueueName = "pagamento-concluido-queue";
    private readonly RabbitMqSettings _settings = rabbitOptions.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.Host,
            Port = _settings.Port,
            UserName = _settings.Username,
            Password = _settings.Password
        };

        using var connection = await factory.CreateConnectionAsync(stoppingToken);
        using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var evento = JsonSerializer.Deserialize<PagamentoConcluidoEvent>(message);

                if (evento != null)
                {
                    logger.LogInformation("📩 Resposta de pagamento recebida para o Pedido ID: {PedidoId}", evento.PedidoId);

                    using var scope = serviceProvider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var pedido = await dbContext.Pedidos.FirstOrDefaultAsync(p => p.Id == evento.PedidoId, stoppingToken);

                    if (pedido != null)
                    {
                        pedido.AlterarStatus(evento.Status);
                        await dbContext.SaveChangesAsync(stoppingToken);

                        logger.LogInformation("✅ Pedido ID: {PedidoId} atualizado no banco com sucesso!", evento.PedidoId);
                    }
                    else
                    {
                        logger.LogWarning("⚠️ Pedido ID: {PedidoId} não foi localizado no banco de dados.", evento.PedidoId);
                    }
                }

                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Erro ao atualizar status do pedido após pagamento.");
                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(
            queue: QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}