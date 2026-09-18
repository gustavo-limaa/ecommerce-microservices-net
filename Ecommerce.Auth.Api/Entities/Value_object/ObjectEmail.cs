using System.Text.RegularExpressions;

namespace Ecommerce.Auth.Api.Entities.ValueObjects;

public partial record Email
{
    public string Valor { get; init; }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();

    private Email(string valor) => Valor = valor;

    public static (Email? Email, string Error) Criar(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return (null, "O e-mail não pode ser vazio.");

        var valorTratado = input.Trim().ToLower();

        if (!EmailRegex().IsMatch(valorTratado))
            return (null, "O formato do e-mail é inválido.");

        return (new Email(valorTratado), string.Empty);
    }

    // Permite usar o VO diretamente onde se espera uma string
    public static implicit operator string(Email email) => email.Valor;

    public override string ToString() => Valor;
}