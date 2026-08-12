using Ecommerce.Catalogo.Api.Domain.Entity;

namespace Ecommerce.Catalogo.Api.Domain.Interfaces;

public interface IProdutoRepository
{
    Task<IEnumerable<Produto>> ObterTodosAsync();

    Task<Produto?> ObterPorIdAsync(Guid id);

    Task<IEnumerable<Produto>> ObterPorCategoriaAsync(Guid categoriaId);

    Task AdicionarAsync(Produto produto);

    Task AtualizarAsync(Produto produto);
}