namespace Ecommerce.Pedido.Api.Domain.GlobalErros.Exceptions;

public class ForbiddenException : CostumBaseException
{
    public ForbiddenException(string message) : base(message)
    {
    }
}