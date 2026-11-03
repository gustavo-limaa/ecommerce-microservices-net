using Ecommerce.Pedido.Api.Domain;
using Ecommerce.Pedido.Api.Infrastructure;
using Ecommerce.Pedido.Api.Infrastructure.Data;
using Ecommerce.Pedido.Api.Mensageria.Events;
using Ecommerce.Pedido.Api.Mensageria.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Ecommerce.Pedido.Api.BackgroundServices;

public class OutboxPublisherWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    private readonly ILogger<OutboxPublisherWorker> _logger;

    public OutboxPublisherWorker(IServiceProvider serviceProvider, ILogger<OutboxPublisherWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxPublisherWorker iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessarMensagensOutboxAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no loop de processamento do OutboxPublisherWorker.");
            }

            // Aguarda 5 segundos antes da próxima varredura
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }

    private async Task ProcessarMensagensOutboxAsync(CancellationToken cancellationToken)
    {
        // Como o BackgroundService é Singleton, precisamos de um Scope para resolver o AppDbContext (Scoped)
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var eventProcessor = scope.ServiceProvider.GetRequiredService<IEventProcessor>(); // Seu serviço de RabbitMQ

        // 1. Busca mensagens pendentes (limitado a 20 por lote)
        var mensagensPendentes = await dbContext.OutboxMessages
            .Where(m => m.ProcessedOnUtc == null)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(20)
            .ToListAsync(cancellationToken);

        if (!mensagensPendentes.Any())
            return;

        _logger.LogInformation("Encontradas {Count} mensagens pendentes no Outbox.", mensagensPendentes.Count);

        foreach (var mensagem in mensagensPendentes)
        {
            try
            {
                // 2. Desserializa o evento de acordo com o tipo
                if (mensagem.Type == nameof(PedidoCriadoEvento))
                {
                    var evento = JsonSerializer.Deserialize<PedidoCriadoEvento>(mensagem.Content);
                    if (evento != null)
                    {
                        // 3. Publica no RabbitMQ
                        await eventProcessor.PublicarEventoAsync(evento, "pedido-criado-queue", cancellationToken, mensagem.CorrelationId);
                    }
                }

                // 4. Marca como processado com sucesso
                mensagem.MarcarComoProcessado();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao publicar mensagem Outbox {Id}.", mensagem.Id);
                mensagem.RegistrarFalha(ex.Message);
            }
        }

        // 5. Salva o status de processamento/falha no MySQL
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}