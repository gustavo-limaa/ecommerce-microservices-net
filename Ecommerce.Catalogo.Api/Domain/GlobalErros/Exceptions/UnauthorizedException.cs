namespace Ecommerce.Catalogo.Api.Domain.GlobalErros.Exceptions;

public class UnauthorizedException : CostumBaseException
{
    public UnauthorizedException(string message) : base(message)
    {
    }
}