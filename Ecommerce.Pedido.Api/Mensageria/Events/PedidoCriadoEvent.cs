namespace Ecommerce.Pedido.Api.Mensageria.Events;

public sealed record PedidoCriadoEvent(Guid PedidoId, Guid ClienteId, decimal ValorTotal, DateTime DataCriacao);