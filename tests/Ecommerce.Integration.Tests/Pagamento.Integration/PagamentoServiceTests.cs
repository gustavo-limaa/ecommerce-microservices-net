using Ecommerce.Pagamento.Worker.Events;
using Ecommerce.Pagamento.Worker.Service;
using Ecommerce.Pagamento.Worker.utility;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;

namespace Ecommerce.Integration.Tests.Pagamento.Integration;

public class PagamentoServiceTests
{
    private readonly Mock<IMemoryCache> _cacheMock;
    private readonly Mock<ILogger<PagamentoService>> _loggerMock;
    private readonly PagamentoService _service;

    public PagamentoServiceTests()
    {
        _cacheMock = new Mock<IMemoryCache>();
        _loggerMock = new Mock<ILogger<PagamentoService>>();
        _service = new PagamentoService(_loggerMock.Object, _cacheMock.Object);
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
    public async Task PagamentoJaFoiProcessadoAsync_DeveRetornarTrue_QuandoPedidoEstiverNoCache()
    {
        // Arrange
        var pedidoId = Guid.NewGuid();
        object valorRetornoDoCache = true;

        // Simulamos que o IMemoryCache encontrou a chave
        _cacheMock
            .Setup(c => c.TryGetValue(pedidoId, out valorRetornoDoCache))
            .Returns(true);

        // Act
        var resultado = await _service.PagamentoJaFoiProcessadoAsync(pedidoId);

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public async Task PagamentoJaFoiProcessadoAsync_DeveRetornarFalse_QuandoPedidoNaoEstiverNoCache()
    {
        // Arrange
        var pedidoId = Guid.NewGuid();
        object valorRetornoDoCache = null;

        // Simulamos que o cache NÃO encontrou a chave (retorna false)
        _cacheMock
            .Setup(c => c.TryGetValue(pedidoId, out valorRetornoDoCache))
            .Returns(false);

        // Act
        var resultado = await _service.PagamentoJaFoiProcessadoAsync(pedidoId);

        // Assert
        resultado.Should().BeFalse();
    }
}