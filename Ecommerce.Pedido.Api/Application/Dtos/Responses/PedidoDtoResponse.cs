using Ecommerce.Pedido.Api.Application.Dtos.Responses;
using Ecommerce.Pedido.Api.Domain.Common; // Ou onde estiver o seu Enum StatusPedido

namespace Ecommerce.Pedido.Api.Application.Dtos.Responses;

public record PedidoDtoResponse(
    Guid Id,
    Guid ClienteId,
    string CpfCliente,
    StatusPedido Status,
    decimal ValorTotal,
    DateTime DataCriacao,
    List<ItemPedidoDtoResponse> Itens
);