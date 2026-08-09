using System.Text;
using System.Text.Json;
using Ecommerce.Pagamento.Worker.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Ecommerce.UnitarioTests.Pagamento.Unitario;

public class WorkerPagamentoTests
{
    private readonly Mock<ILogger<Ecommerce.Pagamento.Worker.Worker>> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;

    public WorkerPagamentoTests()
    {
        _loggerMock = new Mock<ILogger<Ecommerce.Pagamento.Worker.Worker>>();
        _configurationMock = new Mock<IConfiguration>();
    }

    [Fact]
    public void PedidoCriadoEvent_DeveDesserializarCorretamente_QuandoPayloadForValido()
    {
        // Arrange
        var eventoOriginal = new PedidoCriadoEvent
        {
            PedidoId = Guid.NewGuid(),
            ClienteId = Guid.NewGuid(),
            ValorTotal = 250.00m,
            DataCriacao = DateTime.UtcNow
        };

        var jsonMessage = JsonSerializer.Serialize(eventoOriginal);
        var bytes = Encoding.UTF8.GetBytes(jsonMessage);

        // Act
        var jsonRecebido = Encoding.UTF8.GetString(bytes);
        var eventoDesserializado = JsonSerializer.Deserialize<PedidoCriadoEvent>(jsonRecebido);

        // Assert
        Assert.NotNull(eventoDesserializado);
        Assert.Equal(eventoOriginal.PedidoId, eventoDesserializado.PedidoId);
        Assert.Equal(eventoOriginal.ValorTotal, eventoDesserializado.ValorTotal);
    }

    [Fact]
    public void PedidoCriadoEvent_DeveLancarExcecao_QuandoPayloadForInvalido()
    {
        // Arrange
        var jsonMessageInvalido = "{ \"PedidoId\": \"invalid-guid\", \"ValorTotal\": \"not-a-decimal\" }";
        var bytes = Encoding.UTF8.GetBytes(jsonMessageInvalido);
        // Act & Assert
        var jsonRecebido = Encoding.UTF8.GetString(bytes);
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<PedidoCriadoEvent>(jsonRecebido));
    }

    [Fact]
    public void PedidoCriadoEvent_DeveLancarExcecao_QuandoPayloadForNulo()
    {
        // Arrange
        string jsonMessageNulo = null;
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => JsonSerializer.Deserialize<PedidoCriadoEvent>(jsonMessageNulo));
    }

    [Fact]
    public void PedidoCriadoEvent_DeveLancarExcecao_QuandoPayloadForVazio()
    {
        // Arrange
        string jsonMessageVazio = "";
        // Act & Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<PedidoCriadoEvent>(jsonMessageVazio));
    }
}