namespace Ecommerce.Pedido.Api.Domain.Interface;

public interface IOutboxService
{
    Task SalvarComOutboxAsync<TEntidade, TEvento>(
    TEntidade entidade,
    TEvento evento,
    CancellationToken cancellationToken = default)
    where TEntidade : class
    where TEvento : class;
}