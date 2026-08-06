namespace Ecommerce.Pedido.Api.Domain.GlobalErros.Exceptions
{
    public class BadRequestException : CostumBaseException
    {
        public BadRequestException(string message) : base(message)
        {
        }
    }
}