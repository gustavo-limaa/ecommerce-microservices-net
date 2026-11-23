using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit.Abstractions;

namespace Ecommerce.Performance.Tests.PerformancePedido;

public sealed class PedidoPerformanceTests : PerformanceTestBase
{
    private readonly IConfiguration _configuration;

    public PedidoPerformanceTests(ITestOutputHelper output)
        : base(new Uri("http://localhost:5000"), output)
    {
        _configuration = new ConfigurationBuilder()
               .AddUserSecrets<PedidoPerformanceTests>(optional: true)
               .AddEnvironmentVariables()
               .Build();
    }

    [Fact]
    public async Task HealthLive_DeveResponderRapido()
    {
        await MedirEndpointGetAsync("/health/live", iteracoes: 15, limiteMaximoMediaMs: 200);
    }

    [Fact]
    public async Task HealthReady_DeveResponderRapido()
    {
        await MedirEndpointGetAsync("/health/ready", iteracoes: 15, limiteMaximoMediaMs: 200);
    }

    [Fact]
    public async Task GetPedidos_DeveResponderDentroDoLimite()
    {
        await AutenticarClienteAsync();

        await MedirEndpointGetAsync("/api/pedidos", iteracoes: 15, limiteMaximoMediaMs: 500);
    }

    [Fact]
    public async Task MedirGet_DeveExibirTemposDaRota()
    {
        await AutenticarClienteAsync();

        var resultado = await MedirGetAsync(Client, "/api/pedidos", iteracoes: 3);

        EscreverResultado(resultado);

        resultado.MediaRepeticoesMs.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task MedirGet_ProdutosSincronizados()
    {
        var resultado = await MedirGetAsync(Client, "/api/produtossincronizados", iteracoes: 3);
        EscreverResultado(resultado);
        resultado.MediaRepeticoesMs.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task GetProdutosSincronizados_DeveResponderDentroDoLimite()
    {
        await MedirEndpointGetAsync($"/api/produtossincronizados", iteracoes: 15, limiteMaximoMediaMs: 500);
    }

    private async Task AutenticarClienteAsync()
    {
        var token = await ObterTokenViaLoginAsync(_configuration);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task<string> ObterTokenViaLoginAsync(IConfiguration configuration)
    {
        var email = configuration["PERF_TEST_EMAIL"];
        var senha = configuration["PERF_TEST_PASSWORD"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
            throw new InvalidOperationException("Configure PERF_TEST_EMAIL e PERF_TEST_PASSWORD nos User Secrets ou variáveis de ambiente.");

        var response = await Client.PostAsJsonAsync("/api/auth/login", new { email, senha });
        response.EnsureSuccessStatusCode();

        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = doc.RootElement;

        if (root.TryGetProperty("token", out var tokenProp) && !string.IsNullOrWhiteSpace(tokenProp.GetString()))
            return tokenProp.GetString()!;

        if (root.TryGetProperty("accessToken", out var accessTokenProp) && !string.IsNullOrWhiteSpace(accessTokenProp.GetString()))
            return accessTokenProp.GetString()!;

        throw new InvalidOperationException("Propriedade do token não encontrada no retorno do login.");
    }

    private void EscreverResultado(ResultadoMedicao resultado)
    {
        Output.WriteLine(
            $"{resultado.Rota} -> 1ª: {resultado.PrimeiraChamadaMs:F2} ms | " +
            $"Média rep.: {resultado.MediaRepeticoesMs:F2} ms | " +
            $"Min: {resultado.MinRepeticoesMs:F2} ms | " +
            $"Max: {resultado.MaxRepeticoesMs:F2} ms");
    }
}