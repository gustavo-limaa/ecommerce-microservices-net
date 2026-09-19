using Ecommerce.Auth.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Auth.Api.Data
{
    public class UsuarioDbContext : DbContext
    {
        public UsuarioDbContext(DbContextOptions<UsuarioDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios => Set<Usuario>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(UsuarioDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }
    }
}