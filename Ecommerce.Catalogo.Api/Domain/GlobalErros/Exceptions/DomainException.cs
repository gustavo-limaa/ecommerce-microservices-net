namespace Ecommerce.Catalogo.Api.Domain.GlobalErros.Exceptions
{
    public class DomainException : CostumBaseException
    {
        public DomainException(string message) : base(message)
        {
        }
    }
}