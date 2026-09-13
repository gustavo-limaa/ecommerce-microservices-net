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
        logger.LogInformation("🚀 Iniciando Worker de Pagamentos...");

        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMqSettings:Host"] ?? "localhost",
            Port = int.Parse(configuration["RabbitMqSettings:Port"] ?? "5672"),
            UserName = configuration["RabbitMqSettings:Username"],
            Password = configuration["RabbitMqSettings:Password"]
        };

        IConnection? connection = null;

        while (!stoppingToken.IsCancellationRequested && connection == null)
        {
            try
            {
                logger.LogInformation("🔌 Tentando conectar ao RabbitMQ...");
                connection = await factory.CreateConnectionAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning("⏳ RabbitMQ ainda não está pronto ({Message}). Tentando novamente em 5s...", ex.Message);
                await Task.Delay(5000, stoppingToken);
            }
        }

        if (stoppingToken.IsCancellationRequested || connection == null) return;

        using (connection)
        using (var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken))
        {
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
            logger.LogInformation("✅ Conectado! Worker aguardando mensagens em '{Queue}'", QueueRecebimento);

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}