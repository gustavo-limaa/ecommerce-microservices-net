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

        builder.ConfigureServices(services =>
        {
            // 1. Remove qualquer registro existente de IEventProcessor (Interface e Concreta)
            var descriptors = services.Where(d =>
                d.ServiceType == typeof(IEventProcessor) ||
                d.ImplementationType?.GetInterfaces().Contains(typeof(IEventProcessor)) == true
            ).ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            // 2. Cria o Mock limpo
            var eventProcessorMock = new Mock<IEventProcessor>();

            // 3. Registra a instância do Mock e a Interface apontando para o .Object
            services.AddSingleton(eventProcessorMock);
            services.AddSingleton<IEventProcessor>(sp => sp.GetRequiredService<Mock<IEventProcessor>>().Object);

            // 4. Remove o HostedService / Consumer para evitar background connection
            var consumerDescriptor = services.FirstOrDefault(d =>
                d.ImplementationType == typeof(Ecommerce.Pedido.Api.Mensageria.Services.ProdutoCriadoConsumer));

            if (consumerDescriptor != null)
            {
                services.Remove(consumerDescriptor);
            }
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