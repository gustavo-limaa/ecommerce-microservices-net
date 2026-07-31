using Ecommerce.Pedido.Api.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Pedido.Api.Infrastructure.Data.Mappings;

public sealed class ItemPedidoMapping : IEntityTypeConfiguration<ItemPedido>
{
    public void Configure(EntityTypeBuilder<ItemPedido> builder)
    {
        builder.ToTable("ItensPedido");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.ProdutoId)
            .IsRequired();

        builder.Property(i => i.NomeProduto)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(i => i.Quantidade)
            .IsRequired();

        builder.ComplexProperty(i => i.PrecoUnitario, preco =>
        {
            preco.Property(p => p.Valor)
                 .HasColumnName("PrecoUnitario")
                 .HasPrecision(18, 2)
                 .IsRequired();

            preco.Property(p => p.Moeda)
                 .HasColumnName("Moeda")
                 .HasMaxLength(3)
                 .IsRequired();
        });

        builder.Ignore(i => i.ValorTotal);
    }
}