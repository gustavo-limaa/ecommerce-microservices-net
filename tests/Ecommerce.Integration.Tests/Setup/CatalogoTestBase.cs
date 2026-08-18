using System.Net.Http.Json;

namespace Ecommerce.Integration.Tests.Setup;

public abstract class CatalogoTestBase : IClassFixture<CatalogoWebApplicationFactory>, IAsyncLifetime
{
    protected readonly CatalogoWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected CatalogoTestBase(CatalogoWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        await Factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

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
        content ??= new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        return await Client.PatchAsync(url, content);
    }
}