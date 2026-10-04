using Ecommerce.Auth.Api.Dtos;
using Ecommerce.Integration.Tests.Setup;
using EcommerceDataTest;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace Ecommerce.Integration.Tests.Auth.Integration;

public class AuthPostLogin : AuthTestBase
{
    public AuthPostLogin(AuthWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Login_DeveRetornar200OkEToken_QuandoCredenciaisForemValidas()
    {
        // 1. ARRANGE: Gera e regista o utilizador no banco MySQL de testes
        var (registroDto, loginDto) = DataFactory.GerarCredenciaisValidas();

        var responseRegistro = await PostAsync("api/auth/registrar", registroDto);
        responseRegistro.StatusCode.Should().Be(HttpStatusCode.Created);

        // 2. ACT: Tenta fazer o login com as credenciais criadas
        var responseLogin = await PostAsync("api/auth/login", loginDto);

        // 3. ASSERT: Valida status 200 OK e deserializa a resposta exata
        responseLogin.StatusCode.Should().Be(HttpStatusCode.OK);

        var tokenResponse = await responseLogin.Content.ReadFromJsonAsync<TokenResponseDto>();

        tokenResponse.Should().NotBeNull();
        tokenResponse!.Token.Should().NotBeNullOrEmpty();
        tokenResponse.Email.Should().Be(loginDto.Email);
        tokenResponse.Expiracao.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Login_DeveRetornar401Unauthorized_QuandoSenhaForIncorreta()
    {
        // Arrange: Regista utilizador válido
        var (registroDto, loginDto) = DataFactory.GerarCredenciaisValidas();
        await PostAsync("api/auth/registrar", registroDto);

        // Altera apenas a senha no LoginDto
        var loginComSenhaErrada = loginDto with { Senha = "SenhaIncorreta123!" };

        // Act
        var response = await PostAsync("api/auth/login", loginComSenhaErrada);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_DeveRetornar404NotFoundOu401_QuandoUsuarioNaoExistir()
    {
        // Arrange: Tenta logar com um e-mail aleatório que não foi registado
        var loginInexistente = new LoginDto("naoexiste@email.com", "Senha123!");

        // Act
        var response = await PostAsync("api/auth/login", loginInexistente);

        // Assert (Dependendo da estratégia da tua API: 401 Unauthorized ou 404 Not Found)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.Unauthorized, HttpStatusCode.NotFound);
    }
}