using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace Ecommerce.UnitarioTests.Gateway.Unitario;

public abstract class GatewayTestBase : IClassFixture<GatewayWebApplicationFactory>
{
    protected readonly GatewayWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected GatewayTestBase(GatewayWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    // Helpers
    protected async Task<HttpResponseMessage> GetAsync(string url, string? token = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await Client.SendAsync(request);
    }

    protected async Task<HttpResponseMessage> PostAsync<T>(string url, T content, string? token = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(content)
        };

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await Client.SendAsync(request);
    }
}