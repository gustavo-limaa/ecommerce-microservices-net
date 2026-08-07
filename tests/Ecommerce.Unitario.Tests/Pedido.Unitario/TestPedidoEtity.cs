using Ecommerce.Pedido.Api.Domain.Common;
using Ecommerce.Pedido.Api.Domain.GlobalErros.Exceptions;
using EcommerceDataTest;

namespace Ecommerce.UnitarioTests.Pedido.Unitario;

public class TestPedidoEntity
{
    [Fact]
    public void PedidoEntity_DeveTerPropriedadesCorretas_AoAdicionarItem()
    {
        // Arrange
        var pedido = DataFactory.PedidoFaker.Generate();
        var item = DataFactory.ItemPedidoFaker.Generate();
        var quantidadeInicialItens = pedido.Itens.Count;

        // Act
        pedido.AdicionarItem(item);

        // Assert
        Assert.Contains(item, pedido.Itens);
        Assert.Equal(quantidadeInicialItens + 1, pedido.Itens.Count);
    }

    [Fact]
    public void PedidoEntity_AlterarStatus_DeveAlterarStatusCorretamente()
    {
        // Arrange
        var pedido = DataFactory.PedidoFaker.Generate();

        // Act
        pedido.AlterarStatus(StatusPedido.Aprovado);

        // Assert
        Assert.Equal(StatusPedido.Aprovado, pedido.Status);
    }

    [Fact]
    public void PedidoEntity_CalcularValorTotal_DeveSomarApenasOsItensDoPedido()
    {
        // Arrange (Garante uma instância de Pedido sem itens pré-existentes do Faker)
        var pedido = DataFactory.PedidoFaker.Generate();
        var item1 = DataFactory.ItemPedidoFaker.Generate();
        var item2 = DataFactory.ItemPedidoFaker.Generate();

        // Act
        pedido.AdicionarItem(item1);
        pedido.AdicionarItem(item2);

        // Assert
        var valorEsperado = item1.ValorTotal + item2.ValorTotal;
        Assert.Equal(valorEsperado, pedido.ValorTotal);
    }

    [Fact]
    public void PedidoEntity_AdicionarItem_DeveAdicionarItemCorretamente()
    {
        // Arrange
        var pedido = DataFactory.PedidoFaker.Generate();
        var item = DataFactory.ItemPedidoFaker.Generate();

        // Act
        pedido.AdicionarItem(item);

        // Assert
        Assert.Contains(item, pedido.Itens);
    }

    [Fact]
    public void PedidoEntity_AlterarStatus_DeveLancarExcecaoParaTransicaoInvalida()
    {
        // Arrange
        var pedido = DataFactory.PedidoFaker.Generate();
        pedido.AlterarStatus(StatusPedido.Aprovado);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => pedido.AlterarStatus(StatusPedido.Processando));
        Assert.Equal(DomainMessages.PedidoMSG.AlteracaoNaoPermitida, exception.Message);
    }

    [Fact]
    public void PedidoEntity_AlterarStatus_DeveLancarExcecaoParaTransicaoInvalidaDeAprovadoParaReprovado()
    {
        // Arrange
        var pedido = DataFactory.PedidoFaker.Generate();
        pedido.AlterarStatus(StatusPedido.Aprovado);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => pedido.AlterarStatus(StatusPedido.Reprovado));
        Assert.Equal(DomainMessages.PedidoMSG.AlteracaoNaoPermitida, exception.Message);
    }

    [Fact]
    public void PedidoEntity_AlterarStatus_DeveLancarExcecaoParaTransicaoInvalidaDeReprovadoParaAprovado()
    {
        // Arrange
        var pedido = DataFactory.PedidoFaker.Generate();
        pedido.AlterarStatus(StatusPedido.Reprovado);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => pedido.AlterarStatus(StatusPedido.Aprovado));
        Assert.Equal(DomainMessages.PedidoMSG.AlteracaoNaoPermitida, exception.Message);
    }

    [Fact]
    public void PedidoEntity_Cancelamento_DeveCancelarCorretamente()
    {
        // Arrange
        var pedido = DataFactory.PedidoFaker.Generate();

        // Act
        pedido.Cancelar();

        // Assert
        Assert.Equal(StatusPedido.Cancelado, pedido.Status);
    }

    [Fact]
    public void PedidoEntity_Cancelamento_DeveLancarExcecaoParaStatusInvalido()
    {
        // Arrange
        var pedido = DataFactory.PedidoFaker.Generate();
        pedido.AlterarStatus(StatusPedido.Aprovado);
        pedido.AlterarStatus(StatusPedido.ACaminho);

        // Act & Assert
        var exception = Assert.Throws<DomainException>(() => pedido.Cancelar());
        Assert.Equal(DomainMessages.PedidoMSG.AlteracaoNaoPermitida, exception.Message);
    }
}