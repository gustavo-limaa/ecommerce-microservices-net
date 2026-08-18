using System.Net.Http.Json;
using Ecommerce.Integration.Tests.Setup;
using Xunit;

namespace Ecommerce.Integration.Tests.Setup;

public abstract class PedidoTestBase : IClassFixture<PedidoWebApplicationFactory>, IAsyncLifetime
{
    protected readonly PedidoWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected PedidoTestBase(PedidoWebApplicationFactory factory)
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

    protected async Task<HttpResponseMessage> PatchAsync(string url, HttpContent? content = null)
    {
        // Se não passar body, cria um conteúdo JSON vazio para o HttpClient não reclamar
        content ??= new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        return await Client.PatchAsync(url, content);
    }
}