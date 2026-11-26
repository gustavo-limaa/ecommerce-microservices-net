using Ecommerce.Performance.Tests.PerformanceAuth;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit.Abstractions;

public abstract class PerformanceTestBase : IDisposable
{
    protected readonly HttpClient Client;
    private readonly IConfiguration _configuration;
    protected readonly ITestOutputHelper Output;

    protected PerformanceTestBase(Uri gatewayBaseUri, ITestOutputHelper output)
    {
        Output = output;
        Client = new HttpClient
        {
            BaseAddress = gatewayBaseUri
        };

        _configuration = new ConfigurationBuilder()
           .AddUserSecrets<PerformancePost_AUTH>(optional: true)
           .AddEnvironmentVariables()
           .Build();
    }

    protected async Task MedirEndpointGetAsync(
        string rota,
        int iteracoes = 15,
        double limiteMaximoMediaMs = 500,
        HttpStatusCode statusEsperado = HttpStatusCode.OK)
    {
        var warmup = await Client.GetAsync(rota);
        warmup.StatusCode.Should().Be(statusEsperado);

        var temposMs = new List<long>(iteracoes);

        for (var i = 0; i < iteracoes; i++)
        {
            var sw = Stopwatch.StartNew();
            var response = await Client.GetAsync(rota);
            sw.Stop();

            response.StatusCode.Should().Be(statusEsperado);
            temposMs.Add(sw.ElapsedMilliseconds);
        }

        var minimo = temposMs.Min();
        var media = temposMs.Average();
        var maximo = temposMs.Max();

        Output.WriteLine($"[{GetType().Name}] {rota} -> Min: {minimo} ms | Média: {media:F2} ms | Máx: {maximo} ms");

        media.Should().BeLessThan(limiteMaximoMediaMs);
    }

    public virtual void Dispose()
    {
        Client.Dispose();
    }

    public record ResultadoMedicao(
    string Rota,
    double PrimeiraChamadaMs,
    double MediaRepeticoesMs,
    double MinRepeticoesMs,
    double MaxRepeticoesMs);

    public static async Task<ResultadoMedicao> MedirGetAsync(
        HttpClient client,
        string rota,
        int iteracoes)
    {
        var swPrimeira = Stopwatch.StartNew();
        var primeira = await client.GetAsync(rota);
        swPrimeira.Stop();
        primeira.EnsureSuccessStatusCode();

        var repeticoes = new List<double>(iteracoes);

        for (var i = 0; i < iteracoes; i++)
        {
            var sw = Stopwatch.StartNew();
            var resposta = await client.GetAsync(rota);
            sw.Stop();
            resposta.EnsureSuccessStatusCode();

            repeticoes.Add(sw.Elapsed.TotalMilliseconds);
        }

        return new ResultadoMedicao(
            rota,
            swPrimeira.Elapsed.TotalMilliseconds,
            repeticoes.Average(),
            repeticoes.Min(),
            repeticoes.Max());
    }

    public record ResultadoEscritaMs(
    string MetodoHttp,
    string Rota,
    double DuracaoMs,
    HttpResponseMessage Resposta);

    protected static async Task<ResultadoEscritaMs> MedirChamadaEscritaAsync(
        HttpClient client,
        HttpMethod metodo,
        string rota,
        HttpContent? content = null)
    {
        using var request = new HttpRequestMessage(metodo, rota)
        {
            Content = content
        };

        var sw = Stopwatch.StartNew();
        var response = await client.SendAsync(request);
        sw.Stop();

        return new ResultadoEscritaMs(
            metodo.Method,
            rota,
            sw.Elapsed.TotalMilliseconds,
            response);
    }

    protected async Task AutenticarClienteAsync()
    {
        var email = _configuration["PERF_TEST_EMAIL"];
        var senha = _configuration["PERF_TEST_PASSWORD"]; ; // use a senha esperada pela sua validação de senha

        var loginDto = new { Email = email, Senha = senha };
        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", loginDto);

        if (!loginResponse.IsSuccessStatusCode)
        {
            var registroDto = new
            {
                Nome = "Zander Gustavo",
                Email = email,
                Senha = senha
            };

            var registroResponse = await Client.PostAsJsonAsync("/api/auth/registrar", registroDto);
            registroResponse.EnsureSuccessStatusCode();

            // Se o registro cria como 'Cliente', garanta o perfil 'Admin' antes do login final
            await PromoverUsuarioParaAdminAsync(email);

            loginResponse = await Client.PostAsJsonAsync("/api/auth/login", loginDto);
        }

        loginResponse.EnsureSuccessStatusCode();

        var body = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        var token = body.TryGetProperty("token", out var t) ? t.GetString()
                  : body.TryGetProperty("accessToken", out var at) ? at.GetString()
                  : null;

        token.Should().NotBeNullOrWhiteSpace();

        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task PromoverUsuarioParaAdminAsync(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("E-mail do usuário de teste não configurado.", nameof(email));

        var connectionString = _configuration["AuthConnection"]
            ?? throw new InvalidOperationException("AuthConnection não configurada.");

        await using var connection = new MySqlConnector.MySqlConnection(connectionString);
        await connection.OpenAsync();

        const string sql = """
        UPDATE Usuarios
        SET Perfil = 'Admin'
        WHERE Email = @Email;
        """;

        await using var cmd = new MySqlConnector.MySqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@Email", email);

        var afetadas = await cmd.ExecuteNonQueryAsync();
        afetadas.Should().BeGreaterThan(0, "o usuário recém-registrado deve existir na tabela Usuarios antes de ser promovido");
    }

    public async Task PromoverParaAdminAsync(string email)
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