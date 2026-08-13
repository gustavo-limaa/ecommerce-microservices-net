namespace Ecommerce.Pedido.Api.Mensageria.Services;

public record ProdutoCriadoEvent(
Guid Id,
string Nome,
decimal Preco,
int Estoque,
Guid CategoriaId
);