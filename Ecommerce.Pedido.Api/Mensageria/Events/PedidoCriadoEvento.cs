namespace Ecommerce.Pedido.Api.Mensageria.Events;

public sealed record PedidoCriadoEvento(Guid PedidoId, Guid ClienteId, decimal ValorTotal, DateTime DataCriacao);