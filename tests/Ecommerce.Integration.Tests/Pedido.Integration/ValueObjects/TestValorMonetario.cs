using Ecommerce.Pedido.Api.Domain.GlobalErros.Exceptions;
using Ecommerce.Pedido.Api.Domain.Values.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Integration.Tests.Pedido.Integration.ValueObjects;

public class TestValorMonetario
{
    [Fact]
    public void Criar_DeveLancarExcecao_QuandoValorForNegativo()
    {
        Assert.Throws<DomainException>(() => new ValorMonetario(-10.50m));
    }

    [Fact]
    public void SomarOpreator_DeveSomarValoresCorretamente()
    {
        // Arrange
        var v1 = new ValorMonetario(10.00m);
        var v2 = new ValorMonetario(15.50m);

        // Act
        var resultado = v1 + v2;

        // Assert
        Assert.Equal(25.50m, resultado.Valor);
    }
}