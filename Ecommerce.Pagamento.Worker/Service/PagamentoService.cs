using Ecommerce.Pagamento.Worker.Dtos;
using Ecommerce.Pagamento.Worker.Events;
using Ecommerce.Pagamento.Worker.utility;
using QRCoder;

namespace Ecommerce.Pagamento.Worker.Service;

public class PagamentoService(ILogger<PagamentoService> logger) : IPagamentoService
{
    public async Task<PagamentoConcluidoEvent> ProcessarPagamento(PedidoCriadoEvent pedidoCriadoEvent)
    {
        logger.LogInformation("💳 Processando pagamento Pix para Pedido {PedidoId} | R$ {Valor}",
            pedidoCriadoEvent.PedidoId, pedidoCriadoEvent.ValorTotal);

        var payloadPix = $"00020126580014BR.GOV.BCB.PIX0136{Guid.NewGuid()}5204000053039865405{pedidoCriadoEvent.ValorTotal:00.00}5802BR5913ECOMMERCE_STORE6008BRASILIA62070503***6304";

        var qrCodeBase64 = GerarQrCodeBase64(payloadPix);

        await Task.Delay(1000);

        bool aprovado = pedidoCriadoEvent.ValorTotal != 999.99m;

        logger.LogInformation("✅ Pix processado com sucesso. Status: {Status}", aprovado ? "Aprovado" : "Rejeitado");

        return new PagamentoConcluidoEvent(
            pedidoCriadoEvent.PedidoId,
            Guid.NewGuid(),
            Sucesso: aprovado,
             Status: aprovado ? StatusPagamento.Aprovado : StatusPagamento.Reprovado,
            DateTime.Now,
            payloadPix
            );
    }

    private static string GerarQrCodeBase64(string payload)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        return Convert.ToBase64String(qrCode.GetGraphic(20));
    }
}