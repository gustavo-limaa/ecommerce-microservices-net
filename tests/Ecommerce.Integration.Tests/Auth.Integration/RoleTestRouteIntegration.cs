using Ecommerce.Auth.Api.Dtos;
using Ecommerce.Integration.Tests.Setup;
using EcommerceDataTest;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Integration.Tests.Auth.Integration;

public class RoleTestRouteIntegration : AuthTestBase
{
    public RoleTestRouteIntegration(AuthWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task RegistrarAdmin_DeveRetornarUnauthorized_QuandoNaoEnviadoToken()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;
        var (novoAdmin, _) = DataFactory.GerarCredenciaisValidas("Admin");

        // Act
        var response = await PostAsync("/api/auth/registrar-admin", novoAdmin);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RegistrarAdmin_DeveRetornarForbidden_QuandoUsuarioComumTentarAcessar()
    {
        // Arrange
        var (registroCliente, loginCliente) = DataFactory.GerarCredenciaisValidas("Cliente");
        await PostAsync("/api/auth/registrar", registroCliente);

        var loginResponse = await PostAsync("/api/auth/login", loginCliente);
        loginResponse.EnsureSuccessStatusCode();

        var tokenPayload = await loginResponse.Content.ReadFromJsonAsync<TokenResponseDto>();
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenPayload!.Token);

        var (novoAdmin, _) = DataFactory.GerarCredenciaisValidas("Admin");

        // Act
        var response = await PostAsync("/api/auth/registrar-admin", novoAdmin);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RegistrarAdmin_DeveRetornarCreated_QuandoAdminAutenticadoEnviarRequisicao()
    {
        // Arrange
        var tokenAdmin = GerarJwtAdminTeste();
        Client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenAdmin);

        var (novoAdmin, _) =
            DataFactory.GerarCredenciaisValidas("Admin");

        // Act
        var response = await PostAsync("/api/auth/registrar-admin", novoAdmin);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    private static string GerarJwtAdminTeste()
    {
        const string secretKey = "S3cr3t_K3y_S3cur3_T3st_Envir0nm3nt_2026!";
        var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

        var claims = new[]
        {
        new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.Name, "Admin Teste"),
        new Claim(ClaimTypes.Email, "admin.teste@ecommerce.com"),
        new Claim(ClaimTypes.Role, "Admin")
    };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}