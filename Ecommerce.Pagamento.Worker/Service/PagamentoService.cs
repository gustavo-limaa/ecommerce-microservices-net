using Ecommerce.Pagamento.Worker.Data;
using Ecommerce.Pagamento.Worker.Dtos;
using Ecommerce.Pagamento.Worker.Entity;
using Ecommerce.Pagamento.Worker.Events;
using Ecommerce.Pagamento.Worker.utility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QRCoder;

namespace Ecommerce.Pagamento.Worker.Service;

public class PagamentoService : IPagamentoService
{
    private readonly PaymentDbContext _dbContext;
    private readonly ILogger _logger;

    public PagamentoService(PaymentDbContext DbContext, ILogger<PagamentoService> logger)
    {
        _dbContext = DbContext;
        _logger = logger;
    }

    public async Task<PagamentoConcluidoEvent> ProcessarPagamento(PedidoCriadoEvent pedidoCriadoEvent)
    {
        _logger.LogInformation("💳 Processando pagamento Pix para Pedido {PedidoId} | R$ {Valor}",
            pedidoCriadoEvent.PedidoId, pedidoCriadoEvent.ValorTotal);

        // Gera o texto/cópia-e-cola do Pix
        var payloadPix = $"00020126580014BR.GOV.BCB.PIX0136{Guid.NewGuid()}5204000053039865405{pedidoCriadoEvent.ValorTotal:00.00}5802BR5913ECOMMERCE_STORE6008BRASILIA62070503***6304";

        // Converte o texto para a imagem do QR Code em Base64
        var qrCodeBase64 = GerarQrCodeBase64(payloadPix);

        // Simula o tempo de processamento do gateway de pagamento
        await Task.Delay(1000);

        bool aprovado = pedidoCriadoEvent.ValorTotal != 999.99m;

        _logger.LogInformation("✅ Pix processado com sucesso. Status: {Status}", aprovado ? "Aprovado" : "Rejeitado");

        return new PagamentoConcluidoEvent(
            pedidoCriadoEvent.PedidoId,
            Guid.NewGuid(),
            Sucesso: aprovado,
            Status: aprovado ? StatusPagamento.Aprovado : StatusPagamento.Reprovado,
            DataPagamento: DateTime.UtcNow, // 🎯 Dica: Usa sempre UtcNow em microsserviços para evitar problemas de fuso horário!
            QrCodePix: qrCodeBase64 // 🎯 Correção: Agora sim, enviamos a imagem Base64 para o front-end!
        );
    }

    public async Task<bool> PagamentoJaFoiProcessadoAsync(Guid pedidoId)
    {
        // 🎯 Vai ao banco perguntar se este PedidoId já existe na tabela
        return await _dbContext.PagamentosProcessados
            .AnyAsync(p => p.PedidoId == pedidoId);
    }

    public async Task MarcarComoProcessadoAsync(Guid pedidoId)
    {
        // 🎯 Cria a entidade e salva no MySQL
        var registro = new PagamentoProcessado(pedidoId);

        _dbContext.PagamentosProcessados.Add(registro);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("✅ Pagamento do pedido {PedidoId} registado com sucesso no banco de dados.", pedidoId);
    }

    private static string GerarQrCodeBase64(string payload)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);

        // PngByteQRCode é perfeito para não dar dor de cabeça no Docker/Linux!
        using var qrCode = new PngByteQRCode(qrCodeData);

        // Adicionamos o prefixo do Data URI para o front-end injetar direto no src da <img>
        return $"data:image/png;base64,{Convert.ToBase64String(qrCode.GetGraphic(20))}";
    }
}