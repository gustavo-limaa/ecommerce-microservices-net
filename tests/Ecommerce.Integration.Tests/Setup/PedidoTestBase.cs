using Ecommerce.Integration.Tests.Setup;
using Ecommerce.Pedido.Api;
using Ecommerce.Pedido.Api.Mensageria.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace Ecommerce.Integration.Tests.Setup;

public abstract class PedidoTestBase : IAsyncLifetime
{
    protected readonly PedidoWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected PedidoTestBase(PedidoWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();

        Client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "test-token");
    }

    // InitializeAsync, DisposeAsync e Helpers...

    public async Task InitializeAsync()
    {
        await Factory.ResetDatabaseAsync();

        using var scope = Factory.Services.CreateScope();
        var mockProcessor = scope.ServiceProvider.GetService<Mock<IEventProcessor>>();
        mockProcessor?.Invocations.Clear();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // 💡 Helpers de utilidade para deixar as chamadas do teste ultra legíveis
    protected async Task<HttpResponseMessage> PostAsync<T>(string url, T content)
    {
        var cleanUrl = url.TrimStart('/');
        return await Client.PostAsJsonAsync(cleanUrl, content);
    }

    protected async Task<HttpResponseMessage> GetAsync(string url)
    {
        var cleanUrl = url.TrimStart('/');
        return await Client.GetAsync(cleanUrl);
    }

    protected async Task<HttpResponseMessage> PatchAsync(string url, HttpContent? content = null)
    {
        var cleanUrl = url.TrimStart('/');
        content ??= new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        return await Client.PatchAsync(cleanUrl, content);
    }
}