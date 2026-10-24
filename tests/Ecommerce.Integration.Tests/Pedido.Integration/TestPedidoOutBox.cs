using Ecommerce.Integration.Tests.Setup;
using Ecommerce.Pedido.Api.Application.Dtos.Request;
using Ecommerce.Pedido.Api.Domain.Entity;
using Ecommerce.Pedido.Api.Infrastructure.Data;
using EcommerceDataTest;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Ecommerce.Integration.Tests.Pedido.Integration;

[Collection("PedidoCollection")]
public class TestPedidoOutBox : PedidoTestBase
{
    private readonly Mock<IMemoryCache> _cacheMock;

    public TestPedidoOutBox(PedidoWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task TestPedidoOutBox_Should_Work_Correctly()
    {
        // Arrange 1: Criar um escopo para aceder ao banco de testes
        using var setupScope = Factory.Services.CreateScope();
        var setupDbContext = setupScope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Arrange 2: Inserir um "ProdutoSincronizado" válido no banco
        var produtoFakeId = Guid.NewGuid();
        var produtoSincronizado = new ProdutoSincronizado(produtoFakeId, "Produto Mock", 50.0m, 10, true); // Ajusta ao teu construtor
        setupDbContext.ProdutosSincronizados.Add(produtoSincronizado);
        await setupDbContext.SaveChangesAsync();

        var requestDto = DataFactory.PedidoDtoCreateFaker.Generate();

        var itemCorrigido = requestDto.Itens.First() with { ProdutoId = produtoFakeId };

        requestDto = requestDto with { Itens = new List<ItemPedidoDtoCreate> { itemCorrigido } };

        var response = await PostAsync("/api/pedidos", requestDto);

        // Assert 1: Garantir que a API passou nas validações e retornou Sucesso
        response.EnsureSuccessStatusCode();

        // Assert 2: Validação do Outbox (como desenhamos antes)
        using var assertScope = Factory.Services.CreateScope();
        var dbContext = assertScope.ServiceProvider.GetRequiredService<AppDbContext>();

        var outboxMessage = await dbContext.OutboxMessages.FirstOrDefaultAsync();
        outboxMessage.Should().NotBeNull();
        outboxMessage.ProcessedOnUtc.Should().BeNull();
    }
}