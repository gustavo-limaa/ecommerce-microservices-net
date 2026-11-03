using Ecommerce.Pedido.Api.Domain.Entity;
using Ecommerce.Pedido.Api.Domain.GlobalErros.Middleware;
using Ecommerce.Pedido.Api.Domain.Interface;
using Ecommerce.Pedido.Api.Infrastructure.Data;
using System.Text.Json;

namespace Ecommerce.Pedido.Api.Application.Service
{
    public class OutboxService : IOutboxService
    {
        private readonly AppDbContext _dbContext;
        private readonly ICorrelationIdContext _correlationId;

        public OutboxService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
            _correlationId = new CorrelationIdContext();
        }

        public async Task SalvarComOutboxAsync<TEntidade, TEvento>(
            TEntidade entidade,
            TEvento evento,
            CancellationToken cancellationToken = default)
            where TEntidade : class
            where TEvento : class
        {
            var jsonContent = JsonSerializer.Serialize(evento);
            var outboxMessage = new OutboxMessage(typeof(TEvento).Name, jsonContent, _correlationId.CorrelationId);
            using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                await _dbContext.Set<TEntidade>().AddAsync(entidade, cancellationToken);
                await _dbContext.OutboxMessages.AddAsync(outboxMessage, cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }
}