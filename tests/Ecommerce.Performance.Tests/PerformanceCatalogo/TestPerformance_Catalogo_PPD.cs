using Ecommerce.Catalogo.Api.Application.DTOs;
using Ecommerce.Catalogo.Api.Domain.Entity;
using Ecommerce.Performance.Tests.PerformancePedido;
using EcommerceDataTest;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Ecommerce.Performance.Tests.PerformanceCatalogo;

public class TestPerformance_Catalogo_PPD : PerformanceTestBase
{
    private readonly IConfiguration _configuration;

    public TestPerformance_Catalogo_PPD(ITestOutputHelper output)
        : base(new Uri("http://localhost:5000"), output)
    {
        _configuration = new ConfigurationBuilder()
               .AddUserSecrets<PedidoPerformanceTests>(optional: true)
               .AddEnvironmentVariables()
               .Build();
    }

    [Fact]
    public async Task Patch_Catalogo_produtos_DeveResponderDentroDoLimite()
    {
        await AutenticarClienteAsync();
        Guid? categoriaSetupId = null;
        Guid? produtoCriadoId = null;

        try
        {
            // 1. Setup fora do Stopwatch: cria categoria válida
            var categoriaDto = DataFactory.CriarCategoriaDTOFaker.Generate();
            using var postCategoria = await Client.PostAsJsonAsync("/api/categorias", categoriaDto);
            postCategoria.EnsureSuccessStatusCode();

            var categoriaJson = await postCategoria.Content.ReadAsStringAsync();
            categoriaSetupId = ExtrairIdDoJson(categoriaJson);

            // 2. Prepara o DTO do produto usando categoriaSetupId e mede só o POST
            var produtoDto = DataFactory.CriarProdutoDTOFaker(categoriaSetupId!.Value).Generate();
            using var content = JsonContent.Create(produtoDto);

            var resultado = await MedirChamadaEscritaAsync(Client, HttpMethod.Post, "/api/produtos", content);
            using var resposta = resultado.Resposta;

            Output.WriteLine($"[{resultado.MetodoHttp}] {resultado.Rota} -> {resultado.DuracaoMs:F2} ms | Status: {(int)resposta.StatusCode}");

            resposta.StatusCode.Should().Be(HttpStatusCode.Created);
            resultado.DuracaoMs.Should().BeLessThan(800);

            var produtoJson = await resposta.Content.ReadAsStringAsync();
            produtoCriadoId = ExtrairIdDoJson(produtoJson);
        }
        finally
        {
            // 3. Limpeza fora do Stopwatch: produto primeiro, categoria depois
            if (produtoCriadoId.HasValue)
                await ExecutarDeletePorIdAsync("Produtos", produtoCriadoId.Value.ToString());

            if (categoriaSetupId.HasValue)
                await ExecutarDeletePorIdAsync("Categorias", categoriaSetupId.Value.ToString());
        }
    }

    [Fact]
    public async Task POST_Catalogo_categorias_DeveResponderDentroDoLimite()
    {
        var categoriaDto = DataFactory.CategoriaFaker.Generate();
        var content = JsonContent.Create(categoriaDto);
        Guid? idCriado = null;

        try
        {
            var resultado = await MedirChamadaEscritaAsync(Client, HttpMethod.Post, "/api/categorias", content);
            using var resposta = resultado.Resposta;

            Output.WriteLine($"[{resultado.MetodoHttp}] {resultado.Rota} -> {resultado.DuracaoMs:F2} ms | Status: {(int)resposta.StatusCode}");

            resposta.StatusCode.Should().Be(HttpStatusCode.Created);
            resultado.DuracaoMs.Should().BeLessThan(800);

            var json = await resposta.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(json))
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("id", out var idProp) || root.TryGetProperty("Id", out idProp))
                {
                    if (idProp.ValueKind == JsonValueKind.String && Guid.TryParse(idProp.GetString(), out var parsedId))
                        idCriado = parsedId;
                }
            }
        }
        finally
        {
            if (idCriado.HasValue)
                await ExecutarDeletePorIdAsync("Categorias", idCriado.Value.ToString());
        }
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

    private Task LimparProdutoIdAsync(string id) => ExecutarDeletePorIdAsync("Produtos", id);

    private Task LimparCategoriaAsync(string id) => ExecutarDeletePorIdAsync("Categorias", id);

    private async Task ExecutarDeletePorIdAsync(string tabela, string id)
    {
        var connectionString = _configuration["ConnectionStrings:CatalogoConnection"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Output.WriteLine($"[Cleanup] ConnectionStrings:CatalogoConnection não configurada para limpar {tabela}.");
            return;
        }

        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        var sql = $"DELETE FROM {tabela} WHERE Id = @Id;";
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);

        var linhasAfetadas = await command.ExecuteNonQueryAsync();
        Output.WriteLine($"[Cleanup] {tabela} (Id={id}) -> {linhasAfetadas} linha(s) removida(s).");
    }

    private static Guid? ExtrairIdDoJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (root.TryGetProperty("id", out var idProp) || root.TryGetProperty("Id", out idProp))
        {
            if (idProp.ValueKind == JsonValueKind.String && Guid.TryParse(idProp.GetString(), out var guid))
                return guid;
        }

        return null;
    }
}