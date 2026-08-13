using Ecommerce.Pedido.Api.Domain.Entity;

namespace Ecommerce.Pedido.Api.Domain.Interface
{
    public interface IProdutoSincronizadoRepository
    {
        Task SalvarOuAtualizarAsync(ProdutoSincronizado produto, CancellationToken cancellationToken = default);

        Task<ProdutoSincronizado?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);

        Task<IEnumerable<ProdutoSincronizado>> ObterTodosAsync(CancellationToken cancellationToken = default);
    }
}