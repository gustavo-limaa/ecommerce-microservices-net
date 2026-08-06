namespace Ecommerce.Pedido.Api.Domain.GlobalErros.Exceptions
{
    public class ConflictException : CostumBaseException
    {
        public ConflictException(string message) : base(message)
        {
        }
    }
}