using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PedidoEntity = global::Ecommerce.Pedido.Api.Domain.Entity.Pedido;

namespace Ecommerce.Pedido.Api.Infrastructure.Data.Mappings;

public sealed class PedidoMapping : IEntityTypeConfiguration<PedidoEntity>
{
    public void Configure(EntityTypeBuilder<PedidoEntity> builder)
    {
        builder.ToTable("Pedidos");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.ClienteId)
            .IsRequired();

        builder.Property(p => p.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.DataCriacao)
            .IsRequired();

        builder.ComplexProperty(p => p.CpfCliente, cpf =>
        {
            cpf.Property(c => c.Valor)
               .HasColumnName("ClienteCpf")
               .HasMaxLength(11)
               .IsRequired();
        });

        builder.ComplexProperty(p => p.EnderecoEntrega, end =>
        {
            end.Property(e => e.Logradouro).HasColumnName("Endereco_Logradouro").HasMaxLength(200).IsRequired();
            end.Property(e => e.Numero).HasColumnName("Endereco_Numero").HasMaxLength(20).IsRequired();
            end.Property(e => e.Complemento).HasColumnName("Endereco_Complemento").HasMaxLength(100);
            end.Property(e => e.Bairro).HasColumnName("Endereco_Bairro").HasMaxLength(100).IsRequired();
            end.Property(e => e.Cidade).HasColumnName("Endereco_Cidade").HasMaxLength(100).IsRequired();
            end.Property(e => e.Estado).HasColumnName("Endereco_Estado").HasMaxLength(2).IsRequired();
            end.Property(e => e.Cep).HasColumnName("Endereco_Cep").HasMaxLength(8).IsRequired();
        });

        builder.HasMany(p => p.Itens)
            .WithOne()
            .HasForeignKey("PedidoId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(p => p.ValorTotal);
    }
}