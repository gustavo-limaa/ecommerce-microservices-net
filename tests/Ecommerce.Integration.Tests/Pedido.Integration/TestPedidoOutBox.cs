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

    [Fact]
    public async Task OutboxCleanup_Should_Remove_Processed_Messages_Older_Than_Threshold()
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var mensagemAntiga = new OutboxMessage(
            Guid.NewGuid(),
            "PedidoCriado",
            "{}",
            DateTime.UtcNow.AddDays(-10),
            DateTime.UtcNow.AddDays(-8), // Processada há mais de 7 dias -> DEVE SER REMOVIDA
            Guid.NewGuid()
        );

        var mensagemRecenteProcessada = new OutboxMessage(
            Guid.NewGuid(),
            "Teste",
            "{}",
            DateTime.UtcNow.AddHours(-1),
            DateTime.UtcNow.AddMinutes(-30),
            Guid.NewGuid()

        );

        dbContext.OutboxMessages.AddRange(mensagemAntiga, mensagemRecenteProcessada);
        await dbContext.SaveChangesAsync();

        // Act: Executa a mesma lógica de limpeza que o Worker faz
        var dataLimite = DateTime.UtcNow.AddDays(-7);
        var mensagensAntigas = await dbContext.OutboxMessages
            .Where(o => o.ProcessedOnUtc != null && o.ProcessedOnUtc < dataLimite)
            .ToListAsync();

        if (mensagensAntigas.Any())
        {
            dbContext.OutboxMessages.RemoveRange(mensagensAntigas);
            await dbContext.SaveChangesAsync();
        }

        // Assert: A antiga deve sumir, a recente deve continuar lá
        var antigaNoBanco = await dbContext.OutboxMessages.FindAsync(mensagemAntiga.Id);
        antigaNoBanco.Should().BeNull();

        var recenteNoBanco = await dbContext.OutboxMessages.FindAsync(mensagemRecenteProcessada.Id);
        recenteNoBanco.Should().NotBeNull();
    }

    [Fact]
    public async Task OutboxCleanup_Should_Not_Remove_Unprocessed_Messages()
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var mensagemNaoProcessada = new OutboxMessage(
            Guid.NewGuid(),
            "PedidoCriado",
            "{}",
            DateTime.UtcNow.AddDays(-10),
            null, // Não processada
            Guid.NewGuid()
        );
        dbContext.OutboxMessages.Add(mensagemNaoProcessada);
        await dbContext.SaveChangesAsync();
        // Simular a execução do OutboxCleanupWorker
        var cleanupWorker = new Ecommerce.Pedido.Api.Mensageria.Services.OutboxCleanupWorker(scope.ServiceProvider, Mock.Of<Microsoft.Extensions.Logging.ILogger<Ecommerce.Pedido.Api.Mensageria.Services.OutboxCleanupWorker>>());
        await cleanupWorker.StartAsync(CancellationToken.None);
        // Verificar se a mensagem não processada ainda existe
        var mensagemExistente = await dbContext.OutboxMessages.FindAsync(mensagemNaoProcessada.Id);
        mensagemExistente.Should().NotBeNull();
    }

    [Fact]
    public async Task TestPedidoOutBox_Should_Work_failed()
    {         // Arrange 1: Criar um escopo para aceder ao banco de testes
        using var setupScope = Factory.Services.CreateScope();
        var setupDbContext = setupScope.ServiceProvider.GetRequiredService<AppDbContext>();
        // Arrange 2: Inserir um "ProdutoSincronizado" válido no banco
        var produtoFakeId = Guid.NewGuid();
        var produtoSincronizado = new ProdutoSincronizado(produtoFakeId, "Produto Mock", 50.0m, 10, true); // Ajusta ao teu construtor
        setupDbContext.ProdutosSincronizados.Add(produtoSincronizado);
        await setupDbContext.SaveChangesAsync();
        var requestDto = DataFactory.PedidoDtoCreateFaker.Generate();
        var itemCorrigido = requestDto.Itens.First() with { ProdutoId = Guid.NewGuid() };
        requestDto = requestDto with { Itens = new List<ItemPedidoDtoCreate> { itemCorrigido } };
        var response = await PostAsync("/api/pedidos", requestDto);
        // Assert 1: Garantir que a API passou nas validações e retornou Sucesso
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task OutboxCleanup_Should_Never_Remove_Unprocessed_Or_Recent_Messages()
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // 1. Mensagem pendente antiga (NÃO PODE SER APAGADA, pois ProcessedOnUtc é null)
        var mensagemPendenteAntiga = new OutboxMessage(
            Guid.NewGuid(),
            "PedidoCriado",
            "{}",
            DateTime.UtcNow.AddDays(-10),
            null,
            Guid.NewGuid()
        );

        // 2. Mensagem recente processada (NÃO PODE SER APAGADA, pois é recente < 7 dias)
        var mensagemRecenteProcessada = new OutboxMessage(
            Guid.NewGuid(),
            "PedidoCriado",
            "{}",
            DateTime.UtcNow.AddHours(-1),
            DateTime.UtcNow.AddMinutes(-30),
            Guid.NewGuid()

        );

        // 3. Mensagem antiga processada (ESTA SIM DEVE SER APAGADA)
        var mensagemAntigaProcessada = new OutboxMessage(
            Guid.NewGuid(),
            "PedidoCriado",
            "{}",
            DateTime.UtcNow.AddDays(-10),
            DateTime.UtcNow.AddDays(-8),
            Guid.NewGuid()
        );

        dbContext.OutboxMessages.AddRange(mensagemPendenteAntiga, mensagemRecenteProcessada, mensagemAntigaProcessada);
        await dbContext.SaveChangesAsync();

        // Act: Executa a query de limpeza com a blindagem
        var dataLimite = DateTime.UtcNow.AddDays(-7);
        var mensagensParaApagar = await dbContext.OutboxMessages
            .Where(o => o.ProcessedOnUtc != null && o.ProcessedOnUtc < dataLimite)
            .Take(500)
            .ToListAsync();

        if (mensagensParaApagar.Any())
        {
            dbContext.OutboxMessages.RemoveRange(mensagensParaApagar);
            await dbContext.SaveChangesAsync();
        }

        // Assert: As duas primeiras devem continuar firmes e fortes no banco; só a última morre.
        (await dbContext.OutboxMessages.FindAsync(mensagemPendenteAntiga.Id)).Should().NotBeNull();
        (await dbContext.OutboxMessages.FindAsync(mensagemRecenteProcessada.Id)).Should().NotBeNull();
        (await dbContext.OutboxMessages.FindAsync(mensagemAntigaProcessada.Id)).Should().BeNull();
    }
}