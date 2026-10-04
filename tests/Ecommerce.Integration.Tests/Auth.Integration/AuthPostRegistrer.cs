using Ecommerce.Integration.Tests.Setup;
using EcommerceDataTest;
using FluentAssertions;
using System.Net;

namespace Ecommerce.Integration.Tests.Auth.Integration;

public class AuthPostRegistrer : AuthTestBase
{
    public AuthPostRegistrer(AuthWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Login_DeveRetornar200OkEToken_QuandoUsuarioEstiverRegistrado()
    {
        // 1. ARRANGE: Gera credenciais sincronizadas
        var (registroDto, loginDto) = DataFactory.GerarCredenciaisValidas(perfil: "Cliente");

        // Cadastra o utilizador na API Auth
        var responseRegistro = await PostAsync("api/auth/registrar", registroDto);
        if (responseRegistro.StatusCode != HttpStatusCode.Created)
        {
            var erroDetalhado = await responseRegistro.Content.ReadAsStringAsync();
            throw new Exception($"[Falha no Registro Auth 400]: {erroDetalhado}");
        }
        // 2. ACT: Tenta fazer login com o utilizador recém-criado
        var responseLogin = await PostAsync("api/auth/login", loginDto);

        // 3. ASSERT
        responseLogin.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Registrar_DeveRetornar409Conflict_QuandoEmailJaExistir()
    {
        // 1. ARRANGE: Gera credenciais sincronizadas
        var (registroDto, _) = DataFactory.GerarCredenciaisValidas(perfil: "Cliente");
        // Cadastra o utilizador na API Auth
        var responseRegistro1 = await PostAsync("api/auth/registrar", registroDto);
        if (responseRegistro1.StatusCode != HttpStatusCode.Created)
        {
            var erroDetalhado = await responseRegistro1.Content.ReadAsStringAsync();
            throw new Exception($"[Falha no Registro Auth 400]: {erroDetalhado}");
        }
        // 2. ACT: Tenta registrar novamente com o mesmo e-mail
        var responseRegistro2 = await PostAsync("api/auth/registrar", registroDto);
        // 3. ASSERT
        responseRegistro2.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Registrar_DeveRetornar400BadRequest_QuandoEmailForInvalido()
    {
        // 1. ARRANGE: Gera credenciais com e-mail inválido
        var registroDto = DataFactory.RegistroUsuarioDtoFaker.Generate();
        var registroDtoInvalido = registroDto with { Email = "emailinvalido" };

        // 2. ACT: Tenta registrar o usuário com e-mail inválido
        var response = await PostAsync("api/auth/registrar", registroDtoInvalido);
        // 3. ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Registrar_DeveRetornar400BadRequest_QuandoSenhaForInvalida()
    {
        // 1. ARRANGE: Gera credenciais com senha inválida
        var registroDto = DataFactory.RegistroUsuarioDtoFaker.Generate();
        var registroDtoInvalido = registroDto with { Senha = "123" }; // Senha muito curta
        // 2. ACT: Tenta registrar o usuário com senha inválida
        var response = await PostAsync("api/auth/registrar", registroDtoInvalido);
        // 3. ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Registrar_DeveRetornar400BadRequest_QuandoNomeForVazio()
    {
        // 1. ARRANGE: Gera credenciais com nome vazio
        var registroDto = DataFactory.RegistroUsuarioDtoFaker.Generate();
        var registroDtoInvalido = registroDto with { Nome = "" }; // Nome vazio
        // 2. ACT: Tenta registrar o usuário com nome vazio
        var response = await PostAsync("api/auth/registrar", registroDtoInvalido);
        // 3. ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Registrar_DeveRetornar201Created_QuandoEmailForTrue()
    {
        // 1. ARRANGE: Gera credenciais com e-mail vazio
        var registroDto = DataFactory.RegistroUsuarioDtoFaker.Generate();

        // 2. ACT: Tenta registrar o usuário com e-mail vazio
        var response = await PostAsync("api/auth/registrar", registroDto);
        // 3. ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}