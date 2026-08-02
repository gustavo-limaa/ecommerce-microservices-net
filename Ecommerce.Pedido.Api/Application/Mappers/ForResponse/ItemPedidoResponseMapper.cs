using Ecommerce.Pedido.Api.Application.Dtos.Responses;
using Ecommerce.Pedido.Api.Domain.Entity;

namespace Ecommerce.Pedido.Api.Application.Mappers.ForResponse;

public static class ItemPedidoResponseMapper
{
    public static ItemPedidoDtoResponse ToResponse(this ItemPedido item)
    {
        return new ItemPedidoDtoResponse(
            item.ProdutoId,
            item.NomeProduto,
           item.Quantidade,
            item.PrecoUnitario.Valor, // Extrai o decimal do Value Object ValorMonetario
            item.ValorTotal.Valor     // Extrai o decimal do ValorTotal calculado!
        );
    }
}