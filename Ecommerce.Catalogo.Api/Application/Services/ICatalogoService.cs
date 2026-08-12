using Ecommerce.Catalogo.Api.Application.DTOs;

namespace Ecommerce.Catalogo.Api.Application.Services;

public interface ICatalogoService
{
    // Categorias
    Task<IEnumerable<CategoriaResponseDTO>> ObterCategoriasAsync();

    Task<CategoriaResponseDTO> CriarCategoriaAsync(CriarCategoriaDTO dto);

    // Produtos
    Task<IEnumerable<ProdutoResponseDTO>> ObterProdutosAsync();

    Task<ProdutoResponseDTO> ObterProdutoPorIdAsync(Guid id);

    Task<ProdutoResponseDTO> CriarProdutoAsync(CriarProdutoDTO dto);

    Task AtualizarEstoqueAsync(Guid id, int quantidade);
}