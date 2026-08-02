namespace Ecommerce.Pedido.Api.Application.Dtos.Request;

public sealed record class ItemPedidoDtoCreate(Guid ProdutoId,
    string NomeProduto,
    int Quantidade,
    decimal PrecoUnitario
);