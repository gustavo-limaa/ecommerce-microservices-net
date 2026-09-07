using Ecommerce.Pagamento.Worker.utility;
using System;

namespace Ecommerce.Pagamento.Worker.Dtos;

public sealed record PagamentoConcluidoEvent
(
    Guid PedidoId,
    Guid PagamentoId,
    bool Sucesso,
    StatusPagamento Status,
    DateTime DataPagamento,
    string? QrCodePix
);