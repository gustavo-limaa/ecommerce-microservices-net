using Ecommerce.Catalogo.Api.Application.DTOs;
using Ecommerce.Catalogo.Api.Domain.Entity;
using Ecommerce.Performance.Tests.PerformancePedido;
using EcommerceDataTest;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
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
        await GarantirUsuarioAdminAsync();
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

    [Fact]
    public async Task POST_Catalogo_estoque_DeveResponderDentroDoLimite()
    {
        await GarantirUsuarioAdminAsync();

        Guid? categoriaSetupId = null;
        Guid? idCriado = null;

        try
        {
            // 1. Setup fora do Stopwatch: cria uma categoria válida para obter o ID
            var categoriaDto = DataFactory.CriarCategoriaDTOFaker.Generate();
            using var respCategoria = await Client.PostAsJsonAsync("/api/categorias", categoriaDto);
            respCategoria.EnsureSuccessStatusCode();

            var categoriaJson = await respCategoria.Content.ReadAsStringAsync();
            categoriaSetupId = ExtrairIdDoJson(categoriaJson);

            // 2. Monta o produto com o ID da categoria criada e mede só o POST /api/produtos
            var estoqueDto = DataFactory.CriarProdutoDTOFaker(categoriaSetupId!.Value).Generate();
            using var content = JsonContent.Create(estoqueDto);

            var resultado = await MedirChamadaEscritaAsync(Client, HttpMethod.Post, "/api/produtos", content);
            using var resposta = resultado.Resposta;

            Output.WriteLine($"[{resultado.MetodoHttp}] {resultado.Rota} -> {resultado.DuracaoMs:F2} ms | Status: {(int)resposta.StatusCode}");

            resposta.StatusCode.Should().Be(HttpStatusCode.Created);
            resultado.DuracaoMs.Should().BeLessThan(800);

            var json = await resposta.Content.ReadAsStringAsync();
            idCriado = ExtrairIdDoJson(json);

            // 3. Mede só o PATCH /api/produtos/{id}/estoque
            using var patchContent = JsonContent.Create(new { Quantidade = 10 });
            var resultadoPatch = await MedirChamadaEscritaAsync(
                Client,
                HttpMethod.Patch,
                $"/api/produtos/{idCriado}/estoque",
                patchContent);

            using var respostaPatch = resultadoPatch.Resposta;
            Output.WriteLine($"[{resultadoPatch.MetodoHttp}] {resultadoPatch.Rota} -> {resultadoPatch.DuracaoMs:F2} ms | Status: {(int)respostaPatch.StatusCode}");

            respostaPatch.StatusCode.Should().Be(HttpStatusCode.NoContent);
            resultadoPatch.DuracaoMs.Should().BeLessThan(800);
        }
        finally
        {
            if (idCriado.HasValue)
                await ExecutarDeletePorIdAsync("Produtos", idCriado.Value.ToString());

            if (categoriaSetupId.HasValue)
                await ExecutarDeletePorIdAsync("Categorias", categoriaSetupId.Value.ToString());
        }
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