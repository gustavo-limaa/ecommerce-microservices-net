namespace Ecommerce.Pedido.Api.Domain.GlobalErros
{
    public class ApplicationException : Exception
    {
        public ApplicationException(string message) : base(message)
        {
        }
    }
}