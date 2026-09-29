using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using WireMock.Server;

namespace Ecommerce.UnitarioTests.Gateway.Unitario;

public class GatewayWebApplicationFactory : WebApplicationFactory<Program>
{
    public WireMockServer MockServer { get; private set; } = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Sobrescreve as portas dos microsserviços no YARP para evitar tentar conectar em containers em produção
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ReverseProxy:Clusters:auth-cluster:Destinations:destination1:Address", "http://localhost:5999" },
                { "ReverseProxy:Clusters:pedidos-cluster:Destinations:instancia-1:Address", "http://localhost:5998" },
                { "ReverseProxy:Clusters:catalogo-cluster:Destinations:instancia-1:Address", "http://localhost:5997" }
            });
        });
    }

    public override async ValueTask DisposeAsync()
    {
        MockServer.Stop();
        MockServer.Dispose();
        await base.DisposeAsync();
    }
}