using Ecommerce.UnitarioTests.Gateway.Unitario;
using EcommerceDataTest;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;
using Wmhelp.XPath2.AST;

namespace Ecommerce.UnitarioTests.Gateway.Unitario;

public class TestRostasAtravesGateway : GatewayTestBase
{
    public TestRostasAtravesGateway(GatewayWebApplicationFactory factory) : base(factory)
    {
        Factory.MockAuth.Reset();
        Factory.MockPedidos.Reset();
        Factory.MockCatalogo.Reset();
    }

    [Fact]
    public async Task DeveRedirecionarParaAuth_QuandoRotaForLogin()
    {
        // 1. ARRANGE: Mapeia o WireMock do Auth para aceitar o POST de Login na porta 5999
        Factory.MockAuth.ResetMappings();
        Factory.MockAuth
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/auth/login")
                .UsingPost())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{\"token\": \"token_fake_exemplo\"}"));

        // 2. ACT: Faz o POST pelo Gateway
        var payload = new { Email = "teste@email.com", Senha = "Senha123!" };
        var response = await PostAsync("/api/auth/login", payload);

        // 3. ASSERT: O Gateway redireciona pro WireMock e retorna 200 OK
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeveRedirecionarParaAuth_QuandoRotaForRegistrar()
    {
        // 1. ARRANGE: Mapeia o WireMock do Auth para aceitar o POST de Registro na porta 5999
        Factory.MockAuth.ResetMappings();
        Factory.MockAuth
            .Given(WireMock.RequestBuilders.Request.Create()
                .WithPath("/api/auth/registrar")
                .UsingPost())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(201)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{\"id\": 1, \"email\": \"novo@email.com\"}"));

        // 2. ACT: Faz o POST pelo Gateway
        var payload = new { Nome = "Usuario Teste", Email = "novo@email.com", Senha = "Senha123!" };
        var response = await PostAsync("/api/auth/registrar", payload);

        // 3. ASSERT: O Gateway redireciona pro WireMock e retorna 201 Created
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task DeveRetornar502BadGateway_QuandoServicoDeDestinoEstiverForaDoAr()
    {
        // 1. ARRANGE: Desliga o servidor de Mock do Catálogo para simular queda completa da API
        Factory.MockCatalogo.Stop();

        try
        {
            // 2. ACT: O Gateway tenta enviar a requisição para http://localhost:5997 que está fechado
            var response = await GetAsync("/api/produtos/listar");

            // 3. ASSERT: Como não há nada escutando na porta 5997, o YARP retorna Bad Gateway (502)
            response.StatusCode.Should().Be(HttpStatusCode.BadGateway,
                because: "o microserviço de Catálogo estava totalmente fora do ar");
        }
        finally
        {
            // 4. CLEANUP
            Factory.ReiniciarMockCatalogo();
        }
    }

    [Fact]
    public async Task DeveRedirecionarParaPedidos_ComHeaderBearer_QuandoRotaForProtegida()
    {
        Factory.MockPedidos
            .Given(WireMock.RequestBuilders.Request.Create().UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("[\"Pedido1\", \"Pedido2\"]"));

        var tokenValido = GerarJwtTokenValido();
        var response = await GetAsync("/api/pedidos/listar", token: tokenValido);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeveRedirecionarParaProdutos_SemNecessidadeDeToken()
    {
        Factory.MockCatalogo
            .Given(WireMock.RequestBuilders.Request.Create().UsingGet())
            .RespondWith(WireMock.ResponseBuilders.Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("[{\"id\": 1, \"nome\": \"Teclado Gamer\"}]"));

        var response = await GetAsync("/api/produtos/listar");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeveRetornar401Unauthorized_QuandoTokenForInvalido()
    {
        // ACT: Passa uma string qualquer como Token
        var response = await GetAsync("/api/pedidos/listar", token: "token_completamente_invalido_123");

        // ASSERT: O Gateway nem deve chamar o WireMock de Pedidos
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}