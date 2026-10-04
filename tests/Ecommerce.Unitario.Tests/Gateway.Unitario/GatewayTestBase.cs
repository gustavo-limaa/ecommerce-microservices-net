using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
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

    protected string GerarJwtTokenValido()
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("S3cr3t_K3y_S3cur3_T3st_Envir0nm3nt_2026!");

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Email, "teste@email.com")
            }),
            Expires = DateTime.UtcNow.AddHours(2),
            Issuer = "EcommerceAuthApi",
            Audience = "EcommerceClients",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
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