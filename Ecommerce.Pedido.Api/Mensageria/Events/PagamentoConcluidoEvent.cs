namespace Ecommerce.Pedido.Api.Mensageria.Events;

public record PagamentoConcluidoEvent(
    Guid PedidoId,
    Guid PagamentoId,
    bool Sucesso,
    StatusPedido Status,
    DateTime DataPagamento,
    string? QrCodePix
);