using System.Data.Common;
using System.IO;
using Ecommerce.Auth.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using Respawn;
using RespawnTable = Respawn.Graph.Table;
using Xunit;

// 🎯 Garanta o Using com Alias para isolar estritamente o Program da Auth API
using AuthProgram = Ecommerce.Auth.Api.Program;

namespace Ecommerce.Integration.Tests.Setup;

public class AuthWebApplicationFactory : WebApplicationFactory<AuthProgram>, IAsyncLifetime
{
    private DbConnection? _dbConnection;
    private Respawner? _respawner;
    public const string TestConnectionString = "Server=127.0.0.1;Port=3308;Database=ecommerce_auth_testes_db;Uid=test_user;Pwd=test_password_123;";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // 🎯 1. Corrige a localização da pasta raiz da solução para o TestServer não se perder
        var projectDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "Ecommerce.Auth.Api");
        if (Directory.Exists(projectDir))
        {
            builder.UseContentRoot(projectDir);
        }

        // 🎯 2. Injeta as configurações do Host
        builder.UseSetting("ConnectionStrings:DefaultConnection", TestConnectionString);
        builder.UseSetting("JwtSettings:Secret", "S3cr3t_K3y_S3cur3_T3st_Envir0nm3nt_2026!");
        builder.UseSetting("JwtSettings:SecretKey", "S3cr3t_K3y_S3cur3_T3st_Envir0nm3nt_2026!");
        builder.UseSetting("JwtSettings:Issuer", "EcommerceAuthApi");
        builder.UseSetting("JwtSettings:Audience", "EcommerceClients");
        builder.UseSetting("JwtSettings:ExpiracaoHoras", "2");

        // 🎯 3. Injeta as configurações no IConfiguration
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", TestConnectionString },
                { "JwtSettings:Secret", "S3cr3t_K3y_S3cur3_T3st_Envir0nm3nt_2026!" },
                { "JwtSettings:SecretKey", "S3cr3t_K3y_S3cur3_T3st_Envir0nm3nt_2026!" },
                { "JwtSettings:Issuer", "EcommerceAuthApi" },
                { "JwtSettings:Audience", "EcommerceClients" },
                { "JwtSettings:ExpiracaoHoras", "2" }
            });
        });

        // 🎯 4. Reconfigura o DbContext para o MySQL de testes na porta 3308
        builder.ConfigureServices(services =>
        {
            var descriptors = services.Where(d => d.ServiceType == typeof(DbContextOptions<UsuarioDbContext>)).ToList();
            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<UsuarioDbContext>(options =>
                options.UseMySql(TestConnectionString, new MySqlServerVersion(new Version(8, 0, 30))));
        });
    }

    public async Task InitializeAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<UsuarioDbContext>();

        // Roda as migrations na base de testes
        await context.Database.MigrateAsync();

        // Inicializa conexão e Respawner para limpar o banco entre cada teste
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

        await base.DisposeAsync();
    }
}