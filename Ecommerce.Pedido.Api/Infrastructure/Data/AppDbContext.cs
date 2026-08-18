using Microsoft.EntityFrameworkCore;
using PedidoEntity = Ecommerce.Pedido.Api.Domain.Entity.Pedido;
using System.Reflection.Emit;
using Ecommerce.Pedido.Api.Domain.Entity;

namespace Ecommerce.Pedido.Api.Infrastructure.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<PedidoEntity> Pedidos => Set<PedidoEntity>();
    public DbSet<ItemPedido> ItensPedido => Set<ItemPedido>();
    public DbSet<ProdutoSincronizado> ProdutosSincronizados => Set<ProdutoSincronizado>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}