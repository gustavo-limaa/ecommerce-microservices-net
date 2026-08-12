using global::Ecommerce.Catalogo.Api.Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Catalogo.Api.Infra.Data
{
    public class CatalogoDbContext : DbContext
    {
        public CatalogoDbContext(DbContextOptions<CatalogoDbContext> options) : base(options)
        {
        }

        public DbSet<Produto> Produtos => Set<Produto>();
        public DbSet<Categoria> Categorias => Set<Categoria>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogoDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}