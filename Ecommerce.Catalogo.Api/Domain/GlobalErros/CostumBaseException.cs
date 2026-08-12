namespace Ecommerce.Catalogo.Api.Domain.GlobalErros
{
    public class CostumBaseException : Exception

    {
        protected CostumBaseException(string message) : base(message)
        {
        }
    }
}