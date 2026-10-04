using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Auth.Api.Dtos;

public sealed record RegistroUsuarioDto(string Nome, string Email, [StringLength(255, MinimumLength = 6)] string Senha, string Perfil = "Cliente");