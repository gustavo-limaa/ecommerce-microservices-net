namespace Ecommerce.Catalogo.Api.Mensageria.Services
{
    public interface IEventProcessor
    {
        Task PublicarEventoAsync<T>(T evento, string qeueName, CancellationToken cancellationToken = default) where T : class;
    }
}