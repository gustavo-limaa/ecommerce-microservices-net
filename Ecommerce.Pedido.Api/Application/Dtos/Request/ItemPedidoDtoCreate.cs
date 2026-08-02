namespace Ecommerce.Pedido.Api.Aplication.Dtos.Request;

public sealed record class ItemPedidoDtoCreate(Guid ProdutoId,
    string NomeProduto,
    int Quantidade,
    decimal PrecoUnitario
);