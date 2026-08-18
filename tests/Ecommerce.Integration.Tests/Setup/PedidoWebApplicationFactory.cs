using Ecommerce.Pedido.Api.Infrastructure.Data;
using Ecommerce.Pedido.Api.Mensageria.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MySqlConnector;
using Respawn;
using System.Data.Common;
using Xunit;
using RespawnTable = Respawn.Graph.Table;

namespace Ecommerce.Integration.Tests.Setup;

public class PedidoWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private DbConnection? _dbConnection;
    private Respawner? _respawner;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // 1. Configuração de AppConfiguration (User Secrets & ConnectionStrings)
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddUserSecrets<PedidoWebApplicationFactory>();

            var settings = config.Build();
            var connectionString = settings.GetConnectionString("PedidoTestConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("A String de Conexão 'PedidoTestConnection' não foi configurada nos User Secrets!");
            }

            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", connectionString }
            });
        });

        // 2. Configuração de Services (Substituição do RabbitMQ por Mock)
        builder.ConfigureServices(services =>
        {
            // Remove o registro real do RabbitMQ
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IEventProcessor));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Cria o Mock que simula o envio do evento
            var eventProcessorMock = new Mock<IEventProcessor>();
            eventProcessorMock
                .Setup(e => e.PublicarEventoAsync(
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Registra o Mock como Singleton no container de testes
            services.AddSingleton(eventProcessorMock.Object);
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await context.Database.MigrateAsync();

        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("PedidoTestConnection");
        _dbConnection = new MySqlConnection(connectionString);
        await _dbConnection.OpenAsync();

        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.MySql,
            TablesToIgnore = new RespawnTable[] { "__EFMigrationsHistory" }
        });
    }

    public async Task ResetDatabaseAsync()
    {
        if (_dbConnection != null && _respawner != null)
        {
            await _respawner.ResetAsync(_dbConnection);
        }
    }

    public new async Task DisposeAsync()
    {
        if (_dbConnection != null)
        {
            await _dbConnection.CloseAsync();
            await _dbConnection.DisposeAsync();
        }
    }
}