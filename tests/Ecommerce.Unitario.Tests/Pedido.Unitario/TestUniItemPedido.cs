using Ecommerce.Pedido.Api.Domain.Common;
using Ecommerce.Pedido.Api.Domain.Entity;
using Ecommerce.Pedido.Api.Domain.GlobalErros.Exceptions;
using Ecommerce.Pedido.Api.Domain.Values.Objects;
using EcommerceDataTest;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.UnitarioTests.Pedido.Unitario;

public class TestUniItemPedido
{
    [Fact]
    public void ItemPedido_DeveTerPropriedadesCorretas_AoCriarNovo()
    {
        // Arrange
        var item = DataFactory.ItemPedidoFaker.Generate();

        Assert.NotNull(item);
        Assert.Equal(item.ProdutoId, item.ProdutoId);
    }

    [Fact]
    public void ItemPedido_ValorTotal_DeveCalcularCorretamente()
    {
        // Arrange
        var precoUnitario = new ValorMonetario(100m);
        var quantidade = 3;
        var item = new ItemPedido(Guid.NewGuid(), "Produto Teste", precoUnitario, quantidade);
        // Act
        var valorTotalCalculado = item.ValorTotal;
        // Assert
        Assert.Equal(precoUnitario.Valor * quantidade, valorTotalCalculado.Valor);
    }

    [Fact]
    public void ItemPedido_QuantidadeInvalida_DeveLancarExcecao()
    {
        // Arrange
        var precoUnitario = new ValorMonetario(100m);
        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            new ItemPedido(Guid.NewGuid(), "Produto Teste", precoUnitario, 0));
        Assert.Equal(DomainMessages.ItemPedidoMSG.QuantidadeInvalida, exception.Message);
    }

    [Fact]
    public void ItemPedido_NomeProdutoVazio_DeveLancarExcecao()
    {
        // Arrange
        var precoUnitario = new ValorMonetario(100m);
        // Act & Assert
        var exception = Assert.Throws<DomainException>(() =>
            new ItemPedido(Guid.NewGuid(), "", precoUnitario, 1));
        Assert.Equal(DomainMessages.ItemPedidoMSG.NomeObrigatorio, exception.Message);
    }
}