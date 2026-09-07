using Ecommerce.Pagamento.Worker.Dtos;
using Ecommerce.Pagamento.Worker.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ecommerce.Pagamento.Worker.Service
{
    public interface IPagamentoService
    {
        public Task<PagamentoConcluidoEvent> ProcessarPagamento(PedidoCriadoEvent pedidoCriadoEvent);
    }
}