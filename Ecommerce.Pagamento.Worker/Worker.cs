using System.Text;
using System.Text.Json;
using Ecommerce.Pagamento.Worker.Events;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Ecommerce.Pagamento.Worker;

public class Worker(ILogger<Worker> logger, IConfiguration configuration) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("🚀 Worker de Pagamentos iniciado. Aguardando mensagens...");

        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMqSettings:Host"] ?? "localhost",
            Port = int.Parse(configuration["RabbitMqSettings:Port"] ?? "5672"),
            UserName = configuration["RabbitMqSettings:Username"] ?? "guest",
            Password = configuration["RabbitMqSettings:Password"] ?? "guest"
        };

        using var connection = await factory.CreateConnectionAsync(stoppingToken);
        using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        // Garante que a fila de recebimento existe
        await channel.QueueDeclareAsync(
            queue: "pedido-criado-queue",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            cancellationToken: stoppingToken
        );

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var pedidoEvento = JsonSerializer.Deserialize<PedidoCriadoEvent>(message);

                if (pedidoEvento != null)
                {
                    logger.LogInformation("💳 Processando pagamento para o Pedido ID: {PedidoId} | Valor: R$ {ValorTotal}",
                        pedidoEvento.PedidoId, pedidoEvento.ValorTotal);

                    // Simula a validação/processamento da cobrança
                    await Task.Delay(1000, stoppingToken);

                    logger.LogInformation("✅ Pagamento APROVADO com sucesso para o Pedido ID: {PedidoId}", pedidoEvento.PedidoId);
                }

                // Confirmação manual de leitura da mensagem (ACK)
                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Erro ao processar mensagem de pagamento.");
                await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(
            queue: "pedido-criado-queue",
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken
        );

        // Mantém o Worker vivo escutando a fila até a aplicação ser encerrada
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}