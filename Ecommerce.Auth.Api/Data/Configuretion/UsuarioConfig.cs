using Ecommerce.Auth.Api.Entities;
using Ecommerce.Auth.Api.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Auth.Api.Data.Configuration;

public class UsuarioConfig : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Nome)
            .IsRequired()
            .HasMaxLength(100);

        builder.OwnsOne(u => u.Email, emailBuilder =>
        {
            emailBuilder.Property(e => e.Valor)
                .HasColumnName("Email") // Nome da coluna na tabela
                .IsRequired()
                .HasMaxLength(150);

            // O índice único é aplicado diretamente dentro da propriedade navegada
            emailBuilder.HasIndex(e => e.Valor)
                .IsUnique();
        });
        builder.Property(u => u.SenhaHash)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(u => u.Perfil)
           .HasConversion<string>()
            .HasDefaultValue(PerfilUsuario.Client)
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(u => u.CriadoEm)
    .IsRequired()
    .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
    }
}