using global::Ecommerce.Catalogo.Api.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ecommerce.Catalogo.Api.Infra.Mappings
{
    public class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
    {
        public void Configure(EntityTypeBuilder<Produto> builder)
        {
            builder.ToTable("Produtos");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Nome)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(p => p.Descricao)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(p => p.Preco)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(p => p.Estoque)
                .IsRequired();

            builder.Property(p => p.Ativo)
                .IsRequired();

            builder.HasOne(p => p.Categoria)
                .WithMany()
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}