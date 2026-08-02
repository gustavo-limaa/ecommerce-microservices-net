using Ecommerce.Pedido.Api.Domain.Values.Objects;

namespace Ecommerce.Pedido.Api.Application.Dtos.Responses;

public sealed record class ItemPedidoDtoResponse
(
Guid ProdutoId,
string NomeProduto,
int Quantidade,
decimal PrecoUnitario,
decimal Subtotal
);