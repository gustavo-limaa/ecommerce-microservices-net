using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WireMock.Server;
using WireMock.Settings;
using Xunit;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;
using gatewayprogram = Ecommerce.Gateway.Program;

namespace Ecommerce.UnitarioTests.Gateway.Unitario;

public class GatewayWebApplicationFactory : WebApplicationFactory<gatewayprogram>, IAsyncLifetime
{
    public WireMockServer MockAuth { get; private set; } = null!;
    public WireMockServer MockPedidos { get; private set; } = null!;
    public WireMockServer MockCatalogo { get; private set; } = null!;

    public Task InitializeAsync()
    {
        MockAuth = WireMockServer.Start(5999);
        MockPedidos = WireMockServer.Start(5998);
        MockCatalogo = WireMockServer.Start(5997);

        return Task.CompletedTask;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // 🎯 1. Configurações JWT para o Gateway
        builder.UseSetting("JwtSettings:Secret", "S3cr3t_K3y_S3cur3_T3st_Envir0nm3nt_2026!");
        builder.UseSetting("JwtSettings:SecretKey", "S3cr3t_K3y_S3cur3_T3st_Envir0nm3nt_2026!");
        builder.UseSetting("JwtSettings:Issuer", "EcommerceAuthApi");
        builder.UseSetting("JwtSettings:Audience", "EcommerceClients");

        builder.ConfigureServices(services =>
        {
            // 🎯 2. Registramos o provedor de rotas puramente em memória (fortemente tipado em C#)
            // Isso previne qualquer erro de mesclagem de array/JSON do appsettings.json!
            var routes = new[]
            {
                new RouteConfig
                {
                    RouteId = "auth-route",
                    ClusterId = "auth-cluster",
                    Match = new RouteMatch { Path = "/api/auth/{**catch-all}" }
                },
                new RouteConfig
                {
                    RouteId = "pedidos-route",
                    ClusterId = "pedidos-cluster",
                    AuthorizationPolicy = "Authenticated",
                    Match = new RouteMatch { Path = "/api/pedidos/{**catch-all}" }
                },
                new RouteConfig
                {
                    RouteId = "catalogo-route",
                    ClusterId = "catalogo-cluster",
                    Match = new RouteMatch { Path = "/api/produtos/{**catch-all}" }
                }
            };

            var clusters = new[]
            {
                new ClusterConfig
                {
                    ClusterId = "auth-cluster",
                    Destinations = new Dictionary<string, DestinationConfig>
                    {
                        { "destination1", new DestinationConfig { Address = "http://localhost:5999" } }
                    }
                },
                new ClusterConfig
                {
                    ClusterId = "pedidos-cluster",
                    Destinations = new Dictionary<string, DestinationConfig>
                    {
                        { "instancia-1", new DestinationConfig { Address = "http://localhost:5998" } }
                    }
                },
                new ClusterConfig
                {
                    ClusterId = "catalogo-cluster",
                    Destinations = new Dictionary<string, DestinationConfig>
                    {
                        { "instancia-1", new DestinationConfig { Address = "http://localhost:5997" } }
                    }
                }
            };

            // Remove o leitor de JSON do YARP e força a coleção em memória válida
            services.RemoveAll<IProxyConfigProvider>();
            services.AddSingleton<IProxyConfigProvider>(new InMemoryConfigProvider(routes, clusters));
        });
    }

    public new async Task DisposeAsync()
    {
        MockAuth?.Stop();
        MockAuth?.Dispose();

        MockPedidos?.Stop();
        MockPedidos?.Dispose();

        MockCatalogo?.Stop();
        MockCatalogo?.Dispose();

        await base.DisposeAsync();
    }

    public void ReiniciarMockCatalogo()
    {
        MockCatalogo?.Stop();
        MockCatalogo?.Dispose();
        MockCatalogo = WireMockServer.Start(new WireMockServerSettings
        {
            Port = 5997
        });
    }
}