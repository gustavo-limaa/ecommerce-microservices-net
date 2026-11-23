using FluentAssertions;
using System.Diagnostics;
using System.Net;
using Xunit.Abstractions;

public abstract class PerformanceTestBase : IDisposable
{
    protected readonly HttpClient Client;
    protected readonly ITestOutputHelper Output;

    protected PerformanceTestBase(Uri gatewayBaseUri, ITestOutputHelper output)
    {
        Output = output;
        Client = new HttpClient
        {
            BaseAddress = gatewayBaseUri
        };
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
}