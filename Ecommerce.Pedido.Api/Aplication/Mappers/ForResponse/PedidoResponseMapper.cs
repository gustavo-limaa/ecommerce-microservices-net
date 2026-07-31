using Ecommerce.Pedido.Api.Aplication.Dtos.Responses;
using PedidoEntity = Ecommerce.Pedido.Api.Domain.Entity.Pedido;

namespace Ecommerce.Pedido.Api.Application.Mappers.ForResponse;

public static class PedidoResponseMapper
{
    public static PedidoDtoResponse ToResponse(this PedidoEntity pedido)
    {
        return new PedidoDtoResponse(
            pedido.Id,
            pedido.ClienteId,
            pedido.CpfCliente.Valor, // ou pedido.CpfCliente.ToString()
            pedido.Status,
            pedido.ValorTotal.Valor,
            pedido.DataCriacao,
            pedido.Itens.Select(item => item.ToResponse()).ToList()
        );
    }
}