using Ecommerce.Pagamento.Worker.Data;
using Ecommerce.Pagamento.Worker.Entity;
using Ecommerce.Pagamento.Worker.Events;
using Ecommerce.Pagamento.Worker.Service;
using Ecommerce.Pagamento.Worker.utility;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Ecommerce.Integration.Tests.Pagamento.Integration;

public class PagamentoServiceTests : IDisposable
{
    private readonly Mock<ILogger<PagamentoService>> _loggerMock;
    private readonly PaymentDbContext _context;
    private readonly PagamentoService _service;

    public PagamentoServiceTests()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;

        _context = new PaymentDbContext(options);
        _loggerMock = new Mock<ILogger<PagamentoService>>();

        // Passa o DbContext real (em memória) para o serviço
        _service = new PagamentoService(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task ProcessarPixAsync_DeveGerarQrCodeEStatusAprovado_QuandoValorForValido()
    {
        // Arrange
        var pedidoEvento = new PedidoCriadoEvent
        {
            PedidoId = Guid.NewGuid(),
            ClienteId = Guid.NewGuid(),
            ValorTotal = 100.00m,
            DataCriacao = DateTime.UtcNow
        };

        // Act
        var resultado = await _service.ProcessarPagamento(pedidoEvento);

        // Assert
        resultado.Should().NotBeNull();
        resultado.PedidoId.Should().Be(pedidoEvento.PedidoId);
        resultado.Sucesso.Should().BeTrue();
        resultado.Status.Should().Be(StatusPagamento.Aprovado);
        resultado.QrCodePix.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ProcessarPixAsync_DeveRetornarRecusado_QuandoValorForEspecificoDeFalha()
    {
        // Arrange
        var pedidoEvento = new PedidoCriadoEvent
        {
            PedidoId = Guid.NewGuid(),
            ClienteId = Guid.NewGuid(),
            ValorTotal = 999.99m,
            DataCriacao = DateTime.UtcNow
        };

        // Act
        var resultado = await _service.ProcessarPagamento(pedidoEvento);

        // Assert
        resultado.Sucesso.Should().BeFalse();
        resultado.Status.Should().Be(StatusPagamento.Reprovado);
    }

    [Fact]
    public async Task PagamentoJaFoiProcessadoAsync_DeveRetornarTrue_QuandoPedidoEstiverNoBanco()
    {
        // Arrange
        var pedidoId = Guid.NewGuid();

        // Inserimos manualmente um registo na tabela de pagamentos processados da base em memória
        _context.PagamentosProcessados.Add(new PagamentoProcessado
        (
            pedidoId
        ));
        await _context.SaveChangesAsync();

        // Act
        var resultado = await _service.PagamentoJaFoiProcessadoAsync(pedidoId);

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task PagamentoJaFoiProcessadoAsync_DeveRetornarFalse_QuandoPedidoNaoEstiverNoBanco()
    {
        // Arrange
        var pedidoId = Guid.NewGuid();

        // Act (nenhum registo inserido no banco)
        var resultado = await _service.PagamentoJaFoiProcessadoAsync(pedidoId);

        // Assert
        resultado.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}