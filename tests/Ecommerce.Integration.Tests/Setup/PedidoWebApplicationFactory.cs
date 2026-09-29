using Ecommerce.Pedido.Api;
using Ecommerce.Pedido.Api.Domain.Interface;
using Ecommerce.Pedido.Api.Infrastructure.Data;
using Ecommerce.Pedido.Api.Infrastructure.Repositories;
using Ecommerce.Pedido.Api.Mensageria.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MySqlConnector;
using Respawn;
using System.Data.Common;
using RespawnTable = Respawn.Graph.Table;

namespace Ecommerce.Integration.Tests.Setup;

public class PedidoWebApplicationFactory : WebApplicationFactory<IPedidoAssemblyMarker>, IAsyncLifetime
{
    private DbConnection? _dbConnection;
    private Respawner? _respawner;

    private const string TestConnectionString = "Server=127.0.0.1;Port=3308;Database=ecommerce_pedido_testes_db;Uid=test_user;Pwd=test_password_123;";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // 1. Injeta a string de conexão no builder antes de o Program.cs rodar
        builder.UseSetting("ConnectionStrings:DefaultConnection", TestConnectionString);

        builder.ConfigureServices(services =>
        {
            // 2. Configura a Autenticação de Teste
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "TestScheme";
                options.DefaultChallengeScheme = "TestScheme";
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Bearer", options => { });

            // 3. Garante o registo do Controller de Pedidos
            services.AddControllers()
        .AddApplicationPart(typeof(Ecommerce.Pedido.Api.Controllers.PedidosController).Assembly)
        .AddControllersAsServices();
            // 4. Substitui o DbContext pelo banco de testes
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(TestConnectionString, new MySqlServerVersion(new Version(8, 0, 30))));

            // 5. 🎯 CORRECÇÃO DO RABBITMQ: Remove o IEventProcessor do PEDIDO (não do Catálogo)
            var eventProcessorDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(Ecommerce.Pedido.Api.Mensageria.Services.IEventProcessor));

            if (eventProcessorDescriptor != null)
            {
                services.Remove(eventProcessorDescriptor);
            }

            var eventProcessorMock = new Mock<Ecommerce.Pedido.Api.Mensageria.Services.IEventProcessor>();
            services.AddSingleton(eventProcessorMock.Object);
            services.AddSingleton(eventProcessorMock);

            // Remove TODOS os registros anteriores do DbContext
            var descriptors = services.Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>)).ToList();
            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(TestConnectionString, new MySqlServerVersion(new Version(8, 0, 30))));
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

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