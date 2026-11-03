namespace Ecommerce.Pedido.Api.Domain.GlobalErros.Middleware
{
    public interface ICorrelationIdContext
    {
        Guid CorrelationId { get; }

        void SetCorrelationId(Guid correlationId);
    }
}