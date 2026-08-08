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
        Assert.Equal("01001000", endereco.Cep);
        Assert.Equal("SP", endereco.Estado);
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("123456789")]
    public void Criar_DeveLancarExcecao_QuandoCepInvalido(string cepInvalido)
    {
        Assert.Throws<DomainException>(() => new EnderecoEntrega(
            "Rua", "123", null, "Bairro", "Cidade", "SP", cepInvalido
        ));
    }
}