using Ecommerce.Auth.Api.Dtos;
using Ecommerce.Auth.Api.Entities;
using Ecommerce.Auth.Api.Entities.Enums;
using Ecommerce.Auth.Api.Entities.ValueObjects;
using Ecommerce.Auth.Api.Service;
using Ecommerce.Auth.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Auth.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var authGroup = app.MapGroup("/api/auth");

        authGroup.MapPost("/registrar", Registrar);
        authGroup.MapPost("/login", Login);

        return app;
    }

    private static async Task<IResult> Registrar([FromBody] RegistroUsuarioDto dto)
    {
        var (email, erroEmail) = Email.Criar(dto.Email);
        if (email is null)
            return Results.BadRequest(new { mensagem = erroEmail });

        if (!Enum.TryParse<PerfilUsuario>(dto.Perfil, true, out var perfilEnum))
            return Results.BadRequest(new { mensagem = "Perfil inválido. Use 'Cliente' ou 'Admin'." });

        string senhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha);
        var usuario = new Usuario(dto.Nome, email, senhaHash, perfilEnum);

        return Results.Created($"/api/usuarios/{usuario.Id}", new
        {
            id = usuario.Id,
            nome = usuario.Nome,
            email = usuario.Email.Valor,
            perfil = usuario.Perfil.ToString()
        });
    }

    private static async Task<IResult> Login(
        [FromBody] LoginDto dto,
        [FromServices] ITokenService tokenService)
    {
        var (email, erroEmail) = Email.Criar(dto.Email);
        if (email is null)
            return Results.Unauthorized();

        // Mock temporário para validação rápida
        if (email.Valor != "teste@email.com")
            return Results.Unauthorized();

        string hashMock = BCrypt.Net.BCrypt.HashPassword("123456");
        var usuarioMock = new Usuario("Desenvolvedor", email, hashMock, PerfilUsuario.Admin);

        if (!BCrypt.Net.BCrypt.Verify(dto.Senha, usuarioMock.SenhaHash))
            return Results.Unauthorized();

        string token = tokenService.GerarToken(usuarioMock);

        return Results.Ok(new TokenResponseDto(token, 7200, usuarioMock.Email.Valor));
    }
}