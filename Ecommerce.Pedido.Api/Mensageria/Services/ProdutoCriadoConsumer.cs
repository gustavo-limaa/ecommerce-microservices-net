using Ecommerce.Pedido.Api.Domain.Entity;
using Ecommerce.Pedido.Api.Domain.Interface;
using Ecommerce.Pedido.Api.Mensageria.Configuration;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Ecommerce.Pedido.Api.Mensageria.Services;

public class ProdutoCriadoConsumer : BackgroundService
{
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<ProdutoCriadoConsumer> _logger;
    private readonly IServiceProvider _serviceProvider;

    public ProdutoCriadoConsumer(
        IOptions<RabbitMqSettings> options,
        ILogger<ProdutoCriadoConsumer> logger,
        IServiceProvider serviceProvider)
    {
        _settings = options.Value;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 🎯 1. Liberta o arranque da API (impede que o Kestrel trave se o RabbitMQ demorar)
        await Task.Yield();

        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.Host,
                Port = _settings.Port,
                UserName = _settings.Username,
                Password = _settings.Password
            };

            var connection = await factory.CreateConnectionAsync(stoppingToken);
            var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

            const string queueName = "produto-criado-queue";

            await RabbitMqTopologyBuilder.DeclareQueueWithDlqAsync(channel, queueName, stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var evento = JsonSerializer.Deserialize<ProdutoCriadoEvent>(message);

                    if (evento != null)
                    {
                        _logger.LogInformation(" [x] Produto Recebido no Pedido: {Nome} - Preço: {Preco}", evento.Nome, evento.Preco);

                        using var scope = _serviceProvider.CreateScope();
                        var repo = scope.ServiceProvider.GetRequiredService<IProdutoSincronizadoRepository>();

                        var produto = new ProdutoSincronizado(
                        evento.Id,
                        evento.Nome,
                        evento.Preco,
                        evento.Estoque,
                            ativo: true
                        );

                        await repo.SalvarOuAtualizarAsync(produto);
                    }

                    // Sucesso: Confirma a mensagem (Ack)
                    await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar mensagem da fila {Queue}. Rejeitando e enviando para DLQ...", queueName);

                    // 🎯 3. O NACK: Rejeita a mensagem. Como o requeue é false, ela vai para a DLQ!
                    await channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
                }
            };

            await channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken
            );

            _logger.LogInformation(" [*] Aguardando mensagens na fila: {Queue}", queueName);

            // Mantém o worker escutando até a aplicação ser encerrada
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Worker ProdutoCriadoConsumer foi encerrado.");
        }
    }
}