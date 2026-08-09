namespace Ecommerce.Pedido.Api.Mensageria.Services
{
    public interface IEventProcessor
    {
        Task PublicarEventoAsync<T>(T evento, string qeueName, CancellationToken cancellationToken) where T : class;
    }
}