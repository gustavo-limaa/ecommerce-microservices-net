using System.Net.Http.Json;
using Xunit;

namespace Ecommerce.Integration.Tests.Setup;

public abstract class AuthTestBase : IClassFixture<AuthWebApplicationFactory>, IAsyncLifetime
{
    protected readonly AuthWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected AuthTestBase(AuthWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        // Reseta o banco de dados via Respawn antes de cada teste
        await Factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // Helpers de utilidade mantendo a padronização
    protected async Task<HttpResponseMessage> PostAsync<T>(string url, T content)
    {
        return await Client.PostAsJsonAsync(url, content);
    }

    protected async Task<HttpResponseMessage> GetAsync(string url)
    {
        return await Client.GetAsync(url);
    }
}