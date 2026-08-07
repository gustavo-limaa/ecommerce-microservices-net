using Ecommerce.Pedido.Api.Domain.GlobalErros.Exceptions;
using Ecommerce.Pedido.Api.Domain.Values.Objects;

namespace Ecommerce.Integration.Tests.Pedido.Integration.ValueObjects;

public class TestObjectCPF
{
    [Theory]
    [InlineData("11111111111")] // Sequência repetida
    [InlineData("12345678900")] // Dígito verificador inválido
    [InlineData("12345")]       // Tamanho incorreto
    [InlineData("")]            // Em branco
    public void Criar_DeveLancarDomainException_QuandoCpfInvalido(string cpfInvalido)
    {
        Assert.Throws<DomainException>(() => new ObjectCPF(cpfInvalido));
    }

    [Fact]
    public void Criar_DeveCriarCpfEFormatarCorretamente_QuandoCpfValido()
    {
        // Arrange (CPF Válido com pontuação)
        var cpfValido = "529.982.247-25";

        // Act
        var vo = new ObjectCPF(cpfValido);

        // Assert
        Assert.Equal("52998224725", vo.Valor);
        Assert.Equal("529.982.247-25", vo.Formatar());
    }
}