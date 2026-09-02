using Ecommerce.Integration.Tests.Setup;
using Ecommerce.Pedido.Api.Mensageria.Events;
using Xunit;
using Ecommerce.Pedido.Api.Mensageria.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Ecommerce.Integration.Tests.Pedido.Integration.RabbitMq;

[Collection("PedidoTestCollection")]
public class RabbitMqEventProcessorTest : PedidoTestBase
{
    public RabbitMqEventProcessorTest(PedidoWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Deve_publicar_Evento_Normalmente()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var eventProcessorMock = scope.ServiceProvider.GetRequiredService<Mock<IEventProcessor>>();
        var eventProcessor = scope.ServiceProvider.GetRequiredService<IEventProcessor>();

        eventProcessorMock
            .Setup(x => x.PublicarEventoAsync(
                It.IsAny<PedidoCriadoEvento>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var evento = new PedidoCriadoEvento(Guid.NewGuid(), Guid.NewGuid(), 150.00m, DateTime.Now);

        // Act
        await eventProcessor.PublicarEventoAsync(evento, "testequeue", CancellationToken.None);

        // Assert
        eventProcessorMock.Verify(x => x.PublicarEventoAsync(
            It.IsAny<PedidoCriadoEvento>(),
            "testequeue",
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Quando_Falhar_Ao_Publicar_Evento()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var eventProcessorMock = scope.ServiceProvider.GetRequiredService<Mock<IEventProcessor>>();
        var repo = scope.ServiceProvider.GetRequiredService<IEventProcessor>();

        eventProcessorMock
            .Setup(x => x.PublicarEventoAsync(
                It.IsAny<PedidoCriadoEvento>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Falha ao conectar com o broker do RabbitMQ."));

        var evento = new PedidoCriadoEvento(Guid.NewGuid(), Guid.NewGuid(), 100m, DateTime.Now);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repo.PublicarEventoAsync(evento, "testequeue", CancellationToken.None));
    }
}