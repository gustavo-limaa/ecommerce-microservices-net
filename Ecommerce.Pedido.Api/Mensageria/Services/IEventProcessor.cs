namespace Ecommerce.Pedido.Api.Mensageria.Services
{
    public interface IEventProcessor
    {
        Task PublicarEventoAsync<T>(T evento, string queueName, CancellationToken cancellationToken, Guid correlationId) where T : class;
    }
}