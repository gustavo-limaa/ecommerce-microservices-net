using Ecommerce.Catalogo.Api;
using Ecommerce.Catalogo.Api.Infra.Data;
using Ecommerce.Catalogo.Api.Mensageria.Services;
using Ecommerce.Pedido.Api.Infrastructure.Data;
using Ecommerce.Pedido.Api.Mensageria.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using MySqlConnector;
using Respawn;
using System.Data.Common;
using RespawnTable = Respawn.Graph.Table;

namespace Ecommerce.Integration.Tests.Setup;

public class CatalogoWebApplicationFactory : WebApplicationFactory<ICatalogoAssemblyMarker>, IAsyncLifetime
{
    private const string TestConnectionString = "Server=127.0.0.1;Port=3308;Database=ecommerce_catalogo_testes_db;Uid=test_user;Pwd=test_password_123;";
    private DbConnection? _dbConnection;
    private Respawner? _respawner;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Injeta a string de conexão no builder antes do Program.cs ser avaliado
        builder.UseSetting("ConnectionStrings:DefaultConnection", TestConnectionString);

        builder.ConfigureServices(services =>
        {
            // 1. Remove o registro original do DbContextOptions do Catálogo (CatalogoDbContext)
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<CatalogoDbContext>));

            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            // 2. Registra o CatalogoDbContext apontando explicitamente para o banco de testes
            services.AddDbContext<CatalogoDbContext>(options =>
                options.UseMySql(TestConnectionString, new MySqlServerVersion(new Version(8, 0, 30))));

            // 3. Remove o IEventProcessor real e substitui pelo Mock
            var eventProcessorDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(Ecommerce.Catalogo.Api.Mensageria.Services.IEventProcessor));

            if (eventProcessorDescriptor != null)
            {
                services.Remove(eventProcessorDescriptor);
            }

            var eventProcessorMock = new Mock<Ecommerce.Catalogo.Api.Mensageria.Services.IEventProcessor>();
            services.AddSingleton(eventProcessorMock.Object);
            services.AddSingleton(eventProcessorMock);
            services.AddSingleton<Ecommerce.Catalogo.Api.Mensageria.Services.IEventProcessor>(sp => sp.GetRequiredService<Mock<Ecommerce.Catalogo.Api.Mensageria.Services.IEventProcessor>>().Object);
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CatalogoDbContext>();

        // Executa as migrations na base de testes do Catálogo
        await context.Database.MigrateAsync();

        // Inicializa o Respawner
        _dbConnection = new MySqlConnection(TestConnectionString);
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