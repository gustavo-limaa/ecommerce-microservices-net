using Ecommerce.Pedido.Api.Domain.Interface;
using Ecommerce.Pedido.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using PedidoEntity = global::Ecommerce.Pedido.Api.Domain.Entity.Pedido;

namespace Ecommerce.Pedido.Api.Infrastructure.Repositories;

public sealed class PedidoRepository(AppDbContext context) : IPedidoRepository
{
    public async Task AdicionarAsync(PedidoEntity pedido, CancellationToken cancellationToken = default)
    {
        await context.Pedidos.AddAsync(pedido, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<PedidoEntity?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Pedidos
            .Include(p => p.Itens)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<PedidoEntity>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await context.Pedidos
            .Include(p => p.Itens)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PedidoEntity>> ObterComFiltroAsync(
    StatusPedido? status,
    int pagina = 1,
    int tamanhoPagina = 10,
    CancellationToken cancellationToken = default)
    {
        IQueryable<PedidoEntity> query = context.Pedidos.Include(p => p.Itens);

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        return await query
            .AsNoTracking()
            .OrderByDescending(p => p.DataCriacao)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(cancellationToken);
    }

    public async Task AtualizarAsync(PedidoEntity pedido, CancellationToken cancellationToken = default)
    {
        context.Pedidos.Update(pedido);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}