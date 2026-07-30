using Ecommerce.Pedido.Api.Domain.Common;
using Ecommerce.Pedido.Api.Domain.GlobalErros;

namespace Ecommerce.Pedido.Api.Domain.Values.Objects
{
    public record ValorMonetario
    {
        public decimal Valor { get; private init; }
        public string Moeda { get; private init; } = "BRL";

        public ValorMonetario() { }

        public ValorMonetario(decimal valor, string moeda = "BRL")
        {
            if (valor < 0)
                throw new DomainException(DomainMessages.ValorMonetarioMSG.ValorNegativo);

            Valor = valor;
            Moeda = moeda;
        }

        public static ValorMonetario operator +(ValorMonetario a, ValorMonetario b)
            => new(a.Valor + b.Valor, a.Moeda);
    }
}