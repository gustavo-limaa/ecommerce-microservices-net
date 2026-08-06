namespace Ecommerce.Pedido.Api.Domain.GlobalErros.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}