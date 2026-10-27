using Ecommerce.Pedido.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Pedido.Api.Mensageria.Services;

public class OutboxCleanupWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OutboxCleanupWorker> _logger;
    private readonly TimeSpan _periodo = TimeSpan.FromHours(1); // Roda a cada 1 hora (ou ajusta para testes)

    public OutboxCleanupWorker(IServiceProvider serviceProvider, ILogger<OutboxCleanupWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxCleanupWorker iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await LimparMensagensProcessadasAntigasAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao executar a limpeza da tabela OutboxMessages.");
            }

            await Task.Delay(_periodo, stoppingToken);
        }
    }

    private async Task LimparMensagensProcessadasAntigasAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Define o limite: mensagens processadas há mais de 7 dias (ou 1 minuto em ambiente de teste, se preferires)
        var dataLimite = DateTime.UtcNow.AddDays(-7);
        var mensagensAntigas = await dbContext.OutboxMessages
    .Where(o => o.ProcessedOnUtc != null && o.ProcessedOnUtc < dataLimite)
    .Take(500)
    .ToListAsync(cancellationToken);

        if (mensagensAntigas.Any())
        {
            dbContext.OutboxMessages.RemoveRange(mensagensAntigas);
            await dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Foram removidas {Count} mensagens processadas antigas da tabela OutboxMessages.", mensagensAntigas.Count);
        }
    }
}