using Ecommerce.Pedido.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using Respawn;
using RespawnTable = Respawn.Graph.Table;
using System.Data.Common;
using Xunit;

namespace Ecommerce.Integration.Tests.Setup;

public class PedidoWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private DbConnection? _dbConnection;
    private Respawner? _respawner;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // 1. Carrega os User Secrets do próprio projeto de testes
            config.AddUserSecrets<PedidoWebApplicationFactory>();

            var settings = config.Build();

            // 2. Busca a string do Secret ou usa um fallback caso não encontre
            var connectionString = settings.GetConnectionString("TestConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("A String de Conexão 'TestConnection' não foi configurada nos User Secrets!");
            }

            // 3. Sobrescreve a DefaultConnection para a aplicação usar o banco de testes
            config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "ConnectionStrings:DefaultConnection", connectionString }
        });
        });

        // ... resto das configurações dos serviços
    }

    // 🚀 IAsyncLifetime: Executa ANTES de rodar a suíte de testes
    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Garante que o banco de testes existe e tá com todas as migrations aplicadas
        await context.Database.MigrateAsync();

        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        _dbConnection = new MySqlConnection(connectionString);
        await _dbConnection.OpenAsync();

        // Configura o Respawn para ressetar os dados da tabela em milissegundos
        _respawner = await Respawner.CreateAsync(_dbConnection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.MySql,
            TablesToIgnore = new RespawnTable[] { "__EFMigrationsHistory" }
        });
    }

    // Método auxiliar para resetar a base entre cada teste
    public async Task ResetDatabaseAsync()
    {
        if (_dbConnection != null && _respawner != null)
        {
            await _respawner.ResetAsync(_dbConnection);
        }
    }

    // Executa no final da suíte de testes para fechar a conexão
    public new async Task DisposeAsync()
    {
        if (_dbConnection != null)
        {
            await _dbConnection.CloseAsync();
            await _dbConnection.DisposeAsync();
        }
    }
}