using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Pedido.Api.Domain.Entity
{
    // Entidade local no banco do Pedido
    public class ProdutoSincronizado
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Preco { get; set; }

        public int Estoque { get; set; }
        public bool Ativo { get; set; }
    }
}