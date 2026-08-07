using Ecommerce.Pedido.Api.Domain.GlobalErros.Exceptions;
using Ecommerce.Pedido.Api.Domain.Values.Objects;

namespace Ecommerce.Integration.Tests.Pedido.Integration.ValueObjects;

public class TestEnderecoEntrega
{
    [Fact]
    public void Criar_DeveLimparCepETratarEstado_QuandoDadosValidos()
    {
        // Act
        var endereco = new EnderecoEntrega(
            logradouro: "Rua Das Flores",
            numero: "123",
            complemento: "Apto 101",
            bairro: "Centro",
            cidade: "São Paulo",
            estado: "sp",
            cep: "01001-000"
        );

        // Assert
        Assert.Equal("01001000", endereco.Cep); // Garante que limpou a máscara do CEP
        Assert.Equal("SP", endereco.Estado);   // Garante ToUpper no estado
    }

    [Theory]
    [InlineData("12345")]     // CEP com tamanho menor que 8 dígitos
    [InlineData("123456789")] // CEP com tamanho maior que 8 dígitos
    public void Criar_DeveLancarExcecao_QuandoCepInvalido(string cepInvalido)
    {
        Assert.Throws<DomainException>(() => new EnderecoEntrega(
            "Rua", "123", null, "Bairro", "Cidade", "SP", cepInvalido
        ));
    }
}