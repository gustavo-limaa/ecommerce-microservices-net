using Ecommerce.Pedido.Api.Domain.Common;
using Ecommerce.Pedido.Api.Domain.GlobalErros;
using Ecommerce.Pedido.Api.Domain.GlobalErros.Exceptions;
using System.Text.RegularExpressions;

namespace Ecommerce.Pedido.Api.Domain.Values.Objects;

public partial record ObjectEmail
{
    public string Valor { get; private init; }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();

    public ObjectEmail(string valor)
    {
        var (email, error) = Criar(valor);
        if (email == null) throw new DomainException(error);
        Valor = email.Valor;
    }
    private ObjectEmail()
    {
    }

    private ObjectEmail(string valor, bool validado) => Valor = valor;

    public static (ObjectEmail? Email, string Error) Criar(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return (null, DomainMessages.EmailMSG.Obrigatorio);
        var valorTratado = input.Trim().ToLower();
        if (!EmailRegex().IsMatch(valorTratado)) return (null, DomainMessages.EmailMSG.FormatoInvalido);

        return (new ObjectEmail(valorTratado, true), string.Empty);
    }
}