using Ecommerce.Pedido.Api.Aplication.Dtos.Responses;
using Ecommerce.Pedido.Api.Domain.Common; // Ou onde estiver o seu Enum StatusPedido

namespace Ecommerce.Pedido.Api.Application.Dtos.Responses;

public record PedidoDtoResponse(
    Guid Id,
    Guid ClienteId,
    string CpfCliente, // <-- Adicionado para bater com a entidade Pedido!
    StatusPedido Status,
    decimal ValorTotal,
    DateTime DataCriacao,
    List<ItemPedidoDtoResponse> Itens
);