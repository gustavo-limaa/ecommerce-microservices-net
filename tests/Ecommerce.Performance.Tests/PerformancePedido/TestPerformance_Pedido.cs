using EcommerceDataTest;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System.Configuration;
using System.Net;
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
    public async Task MedirGet_ProdutosSincronizados()
    {
        var resultado = await MedirGetAsync(Client, "/api/produtossincronizados", iteracoes: 3);
        EscreverResultado(resultado);
        resultado.MediaRepeticoesMs.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task MedirGet_DeveExibirTemposDaRota()
    {
        await GarantirUsuarioAdminAsync();

        var resultado = await MedirGetAsync(Client, "/api/pedidos", iteracoes: 3);

        EscreverResultado(resultado);

        resultado.MediaRepeticoesMs.Should().BeGreaterThanOrEqualTo(0);
    }

    private void EscreverResultado(ResultadoMedicao resultado)
    {
        Output.WriteLine(
            $"{resultado.Rota} -> 1ª: {resultado.PrimeiraChamadaMs:F2} ms | " +
            $"Média rep.: {resultado.MediaRepeticoesMs:F2} ms | " +
            $"Min: {resultado.MinRepeticoesMs:F2} ms | " +
            $"Max: {resultado.MaxRepeticoesMs:F2} ms");
    }

    private async Task GarantirUsuarioAdminAsync()
    {
        var senhaAdmin = _configuration["PERF_TEST_PASSWORD"]
            ?? throw new InvalidOperationException("Senha de teste não configurada no User Secrets.");

        var emailAdmin = _configuration["PERF_TEST_EMAIL"] ?? "perf.admin@ecommerce.local";

        var registroDto = new
        {
            Nome = "Perf Admin",
            Email = emailAdmin,
            Senha = senhaAdmin
        };

        using var respRegistro = await Client.PostAsJsonAsync("/api/auth/registrar", registroDto);
        if (respRegistro.StatusCode != HttpStatusCode.Created &&
            respRegistro.StatusCode != HttpStatusCode.Conflict &&
            respRegistro.StatusCode != HttpStatusCode.BadRequest)
        {
            respRegistro.EnsureSuccessStatusCode();
        }

        // 1. Promove antes do login para o token já refletir o perfil Admin
        await PromoverParaAdminAsync(emailAdmin);

        // 2. Faz o login e extrai o token
        var loginDto = new
        {
            Email = emailAdmin,
            Senha = senhaAdmin
        };

        using var respLogin = await Client.PostAsJsonAsync("/api/auth/login", loginDto);
        respLogin.EnsureSuccessStatusCode();

        var jsonLogin = await respLogin.Content.ReadAsStringAsync();
        var token = ExtrairTokenDoJson(jsonLogin)
            ?? throw new InvalidOperationException("Token não encontrado na resposta de /api/auth/login.");

        // 3. Anexa o Bearer token no HttpClient usado pelos testes
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    private static string? ExtrairTokenDoJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        foreach (var propName in new[] { "token", "Token", "accessToken", "AccessToken" })
        {
            if (root.TryGetProperty(propName, out var prop) && prop.ValueKind == JsonValueKind.String)
                return prop.GetString();
        }

        return null;
    }

    private async Task PromoverParaAdminAsync(string email)
    {
        var connectionString = _configuration["ConnectionStrings:AuthConnection"]
            ?? throw new InvalidOperationException("ConnectionStrings:AuthConnection não configurada.");

        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        const string sql = "UPDATE Usuarios SET Perfil = 'Admin' WHERE Email = @Email;";
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Email", email);

        var afetadas = await command.ExecuteNonQueryAsync();
        if (afetadas == 0)
            throw new InvalidOperationException($"Usuário {email} não encontrado para promover a Admin.");
    }
}