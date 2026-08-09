using Ecommerce.Pedido.Api.Domain.GlobalErros.Exceptions;
using Ecommerce.Pedido.Api.Domain.Values.Objects;

namespace Ecommerce.UnitarioTests.Pedido.Unitario.ValueObjects;

public class TestObjectEmail
{
    [Theory]
    [InlineData("emailinvalido")]
    [InlineData("@dominio.com")]
    [InlineData("usuario@")]
    [InlineData("")]
    public void Criar_DeveLancarDomainException_QuandoEmailInvalido(string emailInvalido)
    {
        Assert.Throws<DomainException>(() => new ObjectEmail(emailInvalido));
    }

    [Fact]
    public void Criar_DeveSanitizarEGuardarEmail_QuandoValido()
    {
        // Act
        var email = new ObjectEmail("  USER.TEST@DOMAIN.COM  ");

        // Assert
        Assert.Equal("user.test@domain.com", email.Valor);
    }
}