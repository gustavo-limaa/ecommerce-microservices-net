namespace Ecommerce.Pedido.Api.Domain.GlobalErros
{
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message)
        {
        }
    }
}