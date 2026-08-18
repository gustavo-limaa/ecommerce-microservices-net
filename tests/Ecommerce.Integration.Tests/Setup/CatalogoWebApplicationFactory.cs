using Ecommerce.Catalogo.Api;
using Ecommerce.Catalogo.Api.Infra.Data;
using Ecommerce.Catalogo.Api.Mensageria.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MySqlConnector;
using Respawn;
using System.Data.Common;
using RespawnTable = Respawn.Graph.Table;

namespace Ecommerce.Integration.Tests.Setup;

public class CatalogoWebApplicationFactory : WebApplicationFactory<ICatalogoAssemblyMarker>, IAsyncLifetime
{
    private DbConnection? _dbConnection;
    private Respawner? _respawner;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Carrega os User Secrets do projeto de testes
            config.AddUserSecrets<CatalogoWebApplicationFactory>();
        });

        builder.ConfigureServices((context, services) =>
        {
            // Substitui o RabbitMQ por Mock
            var eventDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IEventProcessor));
            if (eventDescriptor != null)
            {
                services.Remove(eventDescriptor);
            }

            var eventProcessorMock = new Mock<IEventProcessor>();
            eventProcessorMock
                .Setup(e => e.PublicarEventoAsync(
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            services.AddSingleton(eventProcessorMock.Object);
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CatalogoDbContext>();

        // Executa as migrations no banco de testes de forma limpa
        await context.Database.MigrateAsync();

        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("CatalogoTestConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("A String de Conexão 'CatalogoTestConnection' não foi carregada no InitializeAsync().");
        }

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