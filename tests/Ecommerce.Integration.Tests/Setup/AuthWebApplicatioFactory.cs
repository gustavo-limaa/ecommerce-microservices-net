using Ecommerce.Auth.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using Respawn;
using System.Data.Common;
using RespawnTable = Respawn.Graph.Table;

namespace Ecommerce.Integration.Tests.Setup;

public class AuthWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private DbConnection? _dbConnection;
    private Respawner? _respawner;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddUserSecrets<AuthWebApplicationFactory>();

            var settings = config.Build();
            var connectionString = settings.GetConnectionString("AuthTestConnection");

            if (!string.IsNullOrEmpty(connectionString))
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", connectionString }
            });
            }
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UsuarioDbContext>();

        // Roda as migrations no MySQL de testes
        await context.Database.MigrateAsync();

        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var connectionString = configuration.GetConnectionString("AuthTestConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("A String de Conexão 'AuthTestConnection' não foi configurada nos User Secrets!");
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