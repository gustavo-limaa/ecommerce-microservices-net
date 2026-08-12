namespace Ecommerce.Catalogo.Api.Application.DTOs;

public record CriarProdutoDTO(
    string Nome,
    string Descricao,
    decimal Preco,
    int Estoque,
    Guid CategoriaId
);

public record AtualizarEstoqueDTO(int Quantidade);

public record ProdutoResponseDTO(
    Guid Id,
    string Nome,
    string Descricao,
    decimal Preco,
    int Estoque,
    bool Ativo,
    Guid CategoriaId,
    string CategoriaNome
);