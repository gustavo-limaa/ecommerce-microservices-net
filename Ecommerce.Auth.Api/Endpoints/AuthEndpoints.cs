using Ecommerce.Auth.Api.Data;
using Ecommerce.Auth.Api.Dtos;
using Ecommerce.Auth.Api.Entities;
using Ecommerce.Auth.Api.Entities.Enums;
using Ecommerce.Auth.Api.Entities.ValueObjects;
using Ecommerce.Auth.Api.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

    private static async Task<IResult> Registrar(
    [FromBody] RegistroUsuarioDto dto,
    [FromServices] UsuarioDbContext context)
    {
        var (email, erroEmail) = Email.Criar(dto.Email);
        if (email is null)
            return Results.BadRequest(new { mensagem = erroEmail });
        if (string.IsNullOrWhiteSpace(dto.Senha) || dto.Senha.Length < 6)
            return Results.BadRequest(new { mensagem = "A senha deve ter pelo menos 6 caracteres." });
        if (string.IsNullOrWhiteSpace(dto.Nome))
            return Results.BadRequest(new { mensagem = "O nome é obrigatório." });
        if (string.IsNullOrWhiteSpace(dto.Perfil))
            return Results.BadRequest(new { mensagem = "O perfil é obrigatório." });
        if (!Enum.TryParse<PerfilUsuario>(dto.Perfil, true, out var perfilEnum))
            return Results.BadRequest(new { mensagem = "Perfil inválido. Use 'Cliente' ou 'Admin'." });
        if (string.IsNullOrWhiteSpace(dto.Senha))
            return Results.BadRequest(new { mensagem = "A senha é obrigatória." });
        // Verifica se já existe um usuário cadastrado com o mesmo e-mail
        var emailExiste = await context.Usuarios.AnyAsync(u => u.Email.Valor == email.Valor);
        if (emailExiste)
            return Results.Conflict(new { mensagem = "Este e-mail já está em uso." });

        string senhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha);
        var usuario = new Usuario(dto.Nome, email, senhaHash, perfilEnum);

        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        return Results.Created($"/api/usuarios/{usuario.Id}", new
        {
            id = usuario.Id,
            nome = usuario.Nome,
            email = usuario.Email.Valor,
            perfil = usuario.Perfil.ToString()
        });
    }

    // Exemplo no Login:
    private static async Task<IResult> Login(
        [FromBody] LoginDto dto,
        [FromServices] UsuarioDbContext context,
        [FromServices] ITokenService tokenService)
    {
        var (email, erroEmail) = Email.Criar(dto.Email);
        if (email is null)
            return Results.Unauthorized();

        var usuario = await context.Usuarios
            .FirstOrDefaultAsync(u => u.Email.Valor == email.Valor);

        if (usuario is null || !BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.SenhaHash))
            return Results.Unauthorized();

        string token = tokenService.GerarToken(usuario);

        return Results.Ok(new TokenResponseDto(token, 7200, usuario.Email.Valor));
    }
}