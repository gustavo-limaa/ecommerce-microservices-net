using Ecommerce.Pagamento.Worker.Events;
using Ecommerce.Pagamento.Worker.Service;
using Ecommerce.Pagamento.Worker.utility;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Ecommerce.Pagamento.Worker.Tests.Services;

public class PagamentoServiceTests
{
    private readonly Mock<ILogger<PagamentoService>> _loggerMock;
    private readonly PagamentoService _service;

    public PagamentoServiceTests()
    {
        _loggerMock = new Mock<ILogger<PagamentoService>>();
        _service = new PagamentoService(_loggerMock.Object);
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
}