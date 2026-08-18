using Ecommerce.Catalogo.Api.Domain.Entity;

namespace Ecommerce.Catalogo.Api.Domain.Interfaces;

public interface ICategoriaRepository
{
    Task<IEnumerable<Categoria>> ObterTodasAsync();

    Task<Categoria?> ObterPorIdAsync(Guid id);

    Task AdicionarAsync(Categoria categoria);

    Task AtualizarAsync(Categoria categoria);
}