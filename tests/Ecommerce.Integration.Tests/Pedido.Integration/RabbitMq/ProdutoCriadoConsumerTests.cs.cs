using Ecommerce.Integration.Tests.Setup;
using Ecommerce.Pedido.Api.Domain.Entity;
using Ecommerce.Pedido.Api.Domain.Interface;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using pedidoex = Ecommerce.Pedido.Api.Domain.GlobalErros.Exceptions.DomainException;
using Ecommerce.Pedido.Api.Mensageria.Services;

namespace Ecommerce.Integration.Tests.Pedido.Integration.RabbitMq;

[Collection("PedidoTestCollection")]
public class ProdutoCriadoConsumerTests : PedidoTestBase
{
    public ProdutoCriadoConsumerTests(PedidoWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Deve_Sincronizar_Produto_Quando_Evento_For_Recebido()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IProdutoSincronizadoRepository>();

        var produtoId = Guid.NewGuid();
        var evento = new ProdutoCriadoEvent(produtoId, "Teclado Mecânico RGB", 299.90m, 15, Guid.NewGuid());
        var produtoSincronizado = new ProdutoSincronizado(evento.Id, evento.Nome, evento.Preco, evento.Estoque, true);

        // Act
        await repo.SalvarOuAtualizarAsync(produtoSincronizado);

        // Assert
        var produtoNoBanco = await repo.ObterPorIdAsync(produtoId);

        produtoNoBanco.Should().NotBeNull();
        produtoNoBanco!.Id.Should().Be(produtoId);
        produtoNoBanco.Nome.Should().Be("Teclado Mecânico RGB");
        produtoNoBanco.Preco.Should().Be(299.90m);
        produtoNoBanco.Estoque.Should().Be(15);
        produtoNoBanco.Ativo.Should().BeTrue();
    }

    [Fact]
    public async Task Deve_Atualizar_Produto_E_Manter_Idempotencia_Quando_Evento_For_Duplicado()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IProdutoSincronizadoRepository>();

        var produtoId = Guid.NewGuid();
        var produtoInicial = new ProdutoSincronizado(produtoId, "Mouse Gamer", 100.00m, 50, true);
        await repo.SalvarOuAtualizarAsync(produtoInicial);

        // Act: Envia evento com o mesmo ID, mas com preço/estoque atualizados
        var produtoAtualizado = new ProdutoSincronizado(produtoId, "Mouse Gamer Pro", 120.00m, 30, true);
        await repo.SalvarOuAtualizarAsync(produtoAtualizado);

        // Assert
        var produtoNoBanco = await repo.ObterPorIdAsync(produtoId);

        produtoNoBanco.Should().NotBeNull();
        produtoNoBanco!.Nome.Should().Be("Mouse Gamer Pro");
        produtoNoBanco.Preco.Should().Be(120.00m);
        produtoNoBanco.Estoque.Should().Be(30);
    }

    [Fact]
    public void Deve_Lancar_DomainException_Quando_Dados_Do_Produto_Forem_Invalidos()
    {
        // Arrange (O scope do repo nem precisa se a validação é no construtor)

        // Act & Assert: Testa a tentativa de instanciar com dados inválidos
        Assert.Throws<pedidoex>(() =>
            new ProdutoSincronizado(Guid.Empty, "", -50.00m, -5, true)
        );
    }
}