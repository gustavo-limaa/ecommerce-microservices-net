using Ecommerce.Pagamento.Worker.Events;
using Ecommerce.Pagamento.Worker.Service;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Ecommerce.Pagamento.Worker;

public class Worker(
    ILogger<Worker> logger,
    IConfiguration configuration,
    IServiceProvider serviceProvider) : BackgroundService
{
    private const string QueueRecebimento = "pedido-criado-queue";
    private const string QueueResposta = "pagamento-concluido-queue";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("🚀 Worker de Pagamentos aguardando mensagens...");

        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMqSettings:Host"] ?? "localhost",
            Port = int.Parse(configuration["RabbitMqSettings:Port"] ?? "5672"),
            UserName = configuration["RabbitMqSettings:Username"],
            Password = configuration["RabbitMqSettings:Password"]
        };

        using var connection = await factory.CreateConnectionAsync(stoppingToken);
        using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(QueueRecebimento, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);
        await channel.QueueDeclareAsync(QueueResposta, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken);

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
                    using var scope = serviceProvider.CreateScope();
                    var pagamentoService = scope.ServiceProvider.GetRequiredService<IPagamentoService>();

                    var resultadoPagamento = await pagamentoService.ProcessarPagamento(pedidoEvento);

                    var respostaBody = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(resultadoPagamento));
                    await channel.BasicPublishAsync("", QueueResposta, true, respostaBody, stoppingToken);

                    logger.LogInformation("📢 Resposta de pagamento enviada para a fila: {Queue}", QueueResposta);
                }

                await channel.BasicAckAsync(ea.DeliveryTag, false, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Erro no processamento. Rejeitando mensagem...");
                await channel.BasicNackAsync(ea.DeliveryTag, false, false, stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(QueueRecebimento, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
}