namespace Ecommerce.Pagamento.Worker.Events;

public sealed record PedidoCriadoEvent
{
    public Guid PedidoId { get; init; }
    public Guid ClienteId { get; init; }
    public decimal ValorTotal { get; init; }
    public DateTime DataCriacao { get; init; }
}