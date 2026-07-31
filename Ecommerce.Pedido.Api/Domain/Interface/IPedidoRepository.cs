using PedidoEntity = Ecommerce.Pedido.Api.Domain.Entity.Pedido;

namespace Ecommerce.Pedido.Api.Domain.Interface;

public interface IPedidoRepository
{
    Task AdicionarAsync(PedidoEntity pedido, CancellationToken cancellationToken = default);

    Task<PedidoEntity?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IEnumerable<PedidoEntity>> ObterTodosAsync(CancellationToken cancellationToken = default);

    Task AtualizarAsync(PedidoEntity pedido, CancellationToken cancellationToken = default);
}