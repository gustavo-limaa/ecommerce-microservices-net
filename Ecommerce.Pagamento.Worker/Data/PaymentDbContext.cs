using Ecommerce.Pagamento.Worker.Entity;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Pagamento.Worker.Data;

public class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options) : base(options)
    {
    }

    public DbSet<PagamentoProcessado> PagamentosProcessados { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurando a chave primária
        modelBuilder.Entity<PagamentoProcessado>()
            .HasKey(p => p.PedidoId);
    }
}