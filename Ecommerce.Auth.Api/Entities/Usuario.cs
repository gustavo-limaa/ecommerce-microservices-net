using Ecommerce.Auth.Api.Entities.Enums;
using Ecommerce.Auth.Api.Entities.ValueObjects;

namespace Ecommerce.Auth.Api.Entities;

public sealed class Usuario
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public Email Email { get; private set; }

    public string SenhaHash { get; private set; }

    public PerfilUsuario Perfil { get; private set; } = PerfilUsuario.Cliente;
    public DateTime CriadoEm { get; private set; } = DateTime.UtcNow;

    private Usuario()
    { } // EF Core

    public Usuario(string nome, Email email, string senhaHash, PerfilUsuario perfil = PerfilUsuario.Cliente)
    {
        Id = Guid.NewGuid();
        Nome = nome;
        Email = email;
        SenhaHash = senhaHash;
        Perfil = perfil;
        CriadoEm = DateTime.UtcNow;
    }
}