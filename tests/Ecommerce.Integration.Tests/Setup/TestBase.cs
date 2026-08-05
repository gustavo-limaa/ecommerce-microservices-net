using System.Net.Http.Json;
using Ecommerce.Integration.Tests.Setup;
using Xunit;

namespace Ecommerce.Integration.Tests.Setup;

public abstract class TestBase : IClassFixture<PedidoWebApplicationFactory>, IAsyncLifetime
{
    protected readonly PedidoWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected TestBase(PedidoWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    // Reset automático da base via Respawn antes de cada teste rodar
    public async Task InitializeAsync()
    {
        await Factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // 💡 Helpers de utilidade para deixar as chamadas do teste ultra legíveis
    protected async Task<HttpResponseMessage> PostAsync<T>(string url, T content)
    {
        return await Client.PostAsJsonAsync(url, content);
    }

    protected async Task<HttpResponseMessage> GetAsync(string url)
    {
        return await Client.GetAsync(url);
    }
}