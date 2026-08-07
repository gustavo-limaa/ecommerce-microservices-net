using Ecommerce.Pedido.Api.Application.Mappers.ForEntities;
using Ecommerce.Pedido.Api.Application.Service;
using Ecommerce.Pedido.Api.Domain.GlobalErros.Exceptions;
using Ecommerce.Pedido.Api.Domain.Interface;
using EcommerceDataTest;
using Moq;
using PedidoE = Ecommerce.Pedido.Api.Domain.Entity.Pedido;

namespace Ecommerce.UnitarioTests.Pedido.Unitario;

public class ServicePedidoTests
{
    private readonly Mock<IPedidoRepository> _repositoryMock;
    private readonly ServicePedido _service;

    public ServicePedidoTests()
    {
        // 1. Mockamos o repositório
        _repositoryMock = new Mock<IPedidoRepository>();

        // 2. Injetamos o mock no Service
        _service = new ServicePedido(_repositoryMock.Object);
    }

    [Fact]
    public async Task CancelarAsync_DeveLancarNotFoundException_QuandoPedidoNaoExistir()
    {
        // Arrange
        var pedidoId = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.ObterPorIdAsync(pedidoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PedidoE?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.CancelarAsync(pedidoId, CancellationToken.None));

        // Garante que o repositório NUNCA tentou atualizar nada no banco
        _repositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<PedidoE>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CancelarAsync_DeveCancelarPedido_QuandoPedidoExistir()
    {
        // Arrange
        var Pedido = DataFactory.PedidoFaker.Generate();
        _repositoryMock
            .Setup(r => r.ObterPorIdAsync(Pedido.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Pedido);

        // Act
        await _service.CancelarAsync(Pedido.Id, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(r => r.AtualizarAsync(Pedido, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancellarAsync_DeveLancarConfllit_Exception_QuandoPedidoJaCancelado()
    {
        // Arrange
        var pedido = DataFactory.PedidoFaker.Generate();
        pedido.Cancelar(); // Cancelamos o pedido para simular o cenário
        _repositoryMock
            .Setup(r => r.ObterPorIdAsync(pedido.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pedido);
        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.CancelarAsync(pedido.Id, CancellationToken.None));
        // Garante que o repositório NUNCA tentou atualizar nada no banco
        _repositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<PedidoE>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AdicionarPedidoAsync_DeveAdicionarPedido_QuandoPedidoValido()
    {
        // Arrange
        var dto = DataFactory.PedidoDtoCreateFaker.Generate();

        // 🎯 Configuração correta para métodos do repositório que retornam Task (void assíncrono)
        _repositoryMock
            .Setup(r => r.AdicionarAsync(It.IsAny<PedidoE>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.AdicionarPedidoAsync(dto, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dto.ClienteId, result.ClienteId);

        // Garante que o método do repositório foi chamado exatamente 1 vez
        _repositoryMock.Verify(
            r => r.AdicionarAsync(It.IsAny<PedidoE>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task AdicionarPedidoAsync_DeveLancarDomainException_QuandoPedidoInvalido()
    {
        // Arrange
        var pedidoDtoCreate = DataFactory.PedidoDtoCreateFaker.Generate() with { ClienteId = Guid.Empty };
        // Deixamos o pedido inválido para simular o cenário

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() =>
            _service.AdicionarPedidoAsync(pedidoDtoCreate, CancellationToken.None));
    }

    [Fact]
    public async Task ObterPedidoPorIdAsync_DeveRetornarPedido_QuandoPedidoExistir()
    {
        // Arrange
        var pedido = DataFactory.PedidoFaker.Generate();
        _repositoryMock
            .Setup(r => r.ObterPorIdAsync(pedido.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pedido);
        // Act
        var result = await _service.ObterPedidoPorIdAsync(pedido.Id, CancellationToken.None);
        // Assert
        Assert.NotNull(result);
        Assert.Equal(pedido.Id, result!.Id);
    }

    [Fact]
    public async Task ObterPedidoPorIdAsync_DeveRetornarNull_QuandoPedidoNaoExistir()
    {
        // Arrange
        var pedidoId = Guid.NewGuid();
        _repositoryMock
            .Setup(r => r.ObterPorIdAsync(pedidoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PedidoE?)null);
        // Act
        var result = await _service.ObterPedidoPorIdAsync(pedidoId, CancellationToken.None);
        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ObterTodosPedidosAsync_DeveRetornarListaDePedidos_QuandoExistiremPedidos()
    {
        // Arrange
        var pedidos = DataFactory.PedidoFaker.Generate(5);
        _repositoryMock
            .Setup(r => r.ObterTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(pedidos);
        // Act
        var result = await _service.ObterTodosPedidosAsync(CancellationToken.None);
        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count());
    }

    [Fact]
    public async Task ObterTodosPedidosAsync_DeveRetornarListaVazia_QuandoNaoExistiremPedidos()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.ObterTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PedidoE>());
        // Act
        var result = await _service.ObterTodosPedidosAsync(CancellationToken.None);
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task ObterPedidosComFiltroAsync_DeveRetornarPedidosFiltrados_QuandoExistiremPedidos()
    {
        // Arrange
        var pedidos = DataFactory.PedidoFaker.Generate(5);
        var status = StatusPedido.Processando;
        _repositoryMock
            .Setup(r => r.ObterComFiltroAsync(status, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pedidos);
        // Act
        var result = await _service.ObterPedidosComFiltroAsync(status, 1, 10, CancellationToken.None);
        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, result.Count());
    }
}