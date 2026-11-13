using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Pagamento.Worker.Entity;

public class PagamentoProcessado
{
    // O PedidoId será a nossa Chave Primária.
    // Como a chave primária é única no banco, é impossível inserir o mesmo pedido duas vezes!
    public Guid PedidoId { get; private set; }

    public DateTime DataProcessamento { get; private set; }

    // Construtor vazio para o EF Core
    protected PagamentoProcessado()
    { }

    public PagamentoProcessado(Guid pedidoId)
    {
        PedidoId = pedidoId;
        DataProcessamento = DateTime.UtcNow;
    }
}