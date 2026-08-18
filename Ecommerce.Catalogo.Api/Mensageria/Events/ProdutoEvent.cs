namespace Ecommerce.Catalogo.Api.Mensageria.Events;

public sealed record ProdutoCriadoEvent(
    Guid Id,
    string Nome,
    decimal Preco,
    int Estoque,
    Guid CategoriaId
);
public sealed record ProdutoAtualizadoEvent(
    Guid ProdutoId,
    string Nome,
    decimal Preco,
    int Estoque,
    bool Ativo
);