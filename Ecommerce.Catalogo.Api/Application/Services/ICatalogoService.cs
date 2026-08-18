using Ecommerce.Catalogo.Api.Application.DTOs;

namespace Ecommerce.Catalogo.Api.Application.Services;

public interface ICatalogoService
{
    // Categorias
    Task<IEnumerable<CategoriaResponseDTO>> ObterCategoriasAsync();

    Task<CategoriaResponseDTO> ObterCategoriaPorIdAsync(Guid id);

    Task<CategoriaResponseDTO> CriarCategoriaAsync(CriarCategoriaDTO dto);

    // Produtos
    Task<IEnumerable<ProdutoResponseDTO>> ObterProdutosAsync();

    Task<ProdutoResponseDTO> ObterProdutoPorIdAsync(Guid id);

    Task<ProdutoResponseDTO> CriarProdutoAsync(CriarProdutoDTO dto, CancellationToken cancellationToken = default);

    Task AtualizarEstoqueAsync(Guid id, int quantidade);

    Task<IEnumerable<ProdutoResponseDTO>> ObterPaginacaoAsync(int pageNumber, int pageSize);
}