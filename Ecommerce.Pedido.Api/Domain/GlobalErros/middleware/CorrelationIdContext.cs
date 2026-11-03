namespace Ecommerce.Pedido.Api.Domain.GlobalErros.Middleware;

public class CorrelationIdContext : ICorrelationIdContext
{
    public Guid CorrelationId { get; private set; }

    public void SetCorrelationId(Guid correlationId)
    {
        if (CorrelationId != Guid.Empty)
        {
            return;
        }

        CorrelationId = correlationId;
    }
}