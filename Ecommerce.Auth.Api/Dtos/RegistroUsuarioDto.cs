namespace Ecommerce.Auth.Api.Dtos;

public sealed record RegistroUsuarioDto(string Nome, string Email, string Senha, string Perfil = "Client");