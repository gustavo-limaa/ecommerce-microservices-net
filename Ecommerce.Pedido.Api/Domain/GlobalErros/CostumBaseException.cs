namespace Ecommerce.Pedido.Api.Domain.GlobalErros
{
    public class CostumBaseException : Exception

    {
        protected CostumBaseException(string message) : base(message)
        {
        }
    }
}