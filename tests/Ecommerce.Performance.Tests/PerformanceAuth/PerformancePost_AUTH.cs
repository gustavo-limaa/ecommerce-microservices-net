using EcommerceDataTest;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Ecommerce.Performance.Tests.PerformanceAuth;

public class PerformancePost_AUTH : PerformanceTestBase
{
    private readonly IConfiguration _configuration;

    public PerformancePost_AUTH(ITestOutputHelper output) : base(new Uri("http://localhost:5000"), output)
    {
        _configuration = new ConfigurationBuilder()
               .AddUserSecrets<PerformancePost_AUTH>(optional: true)
               .AddEnvironmentVariables()
               .Build();
    }

    [Fact]
    public async Task Post_Login_Usuario_admin()
    {
        await GarantirUsuarioAdminAsync();
        Output.WriteLine("Usuário admin garantido para o teste de login.");
    }

    [Fact]
    public async Task Post_Registrar_DeveCriarDentroDoLimite_ELimparForaDoCronometro()
    {
        var registerDto = DataFactory.RegistroUsuarioDtoFaker.Generate();
        using var content = JsonContent.Create(registerDto);

        try
        {
            // 1. Medição pura (só a chamada POST)
            var resultado = await MedirChamadaEscritaAsync(Client, HttpMethod.Post, "/api/auth/registrar", content);
            using var resposta = resultado.Resposta;

            Output.WriteLine($"[{resultado.MetodoHttp}] {resultado.Rota} -> {resultado.DuracaoMs:F2} ms | Status: {(int)resposta.StatusCode}");

            resposta.StatusCode.Should().Be(HttpStatusCode.Created);
            resultado.DuracaoMs.Should().BeLessThan(800);
        }
        finally
        {
            // 2. Limpeza fora do Stopwatch (ex.: remover o usuário criado pelo e-mail gerado)
            await LimparUsuarioPorEmailAsync(registerDto.Email);
        }
    }

    private async Task LimparUsuarioPorEmailAsync(string email)
    {
        var connectionString = _configuration["ConnectionStrings:AuthConnection"];
        if (string.IsNullOrWhiteSpace(connectionString))
            return; // ou lança exceção se quiser exigir limpeza obrigatória

        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        const string sql = "DELETE FROM Usuarios WHERE Email = @Email;";
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Email", email);

        await command.ExecuteNonQueryAsync();
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

        var loginDto = new
        {
            Email = emailAdmin,
            Senha = senhaAdmin
        };

        using var respLogin = await Client.PostAsJsonAsync("/api/auth/login", loginDto);
        respLogin.EnsureSuccessStatusCode();

        await PromoverParaAdminAsync(emailAdmin);
    }
}