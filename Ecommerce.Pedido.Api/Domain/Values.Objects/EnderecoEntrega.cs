using Ecommerce.Pedido.Api.Domain.Common;
using Ecommerce.Pedido.Api.Domain.GlobalErros;
using Ecommerce.Pedido.Api.Domain.GlobalErros.Exceptions;
using System.Text.RegularExpressions;

namespace Ecommerce.Pedido.Api.Domain.Values.Objects;

public sealed record EnderecoEntrega
{
    public string Logradouro { get; private init; }
    public string Numero { get; private init; }
    public string? Complemento { get; private init; }
    public string Bairro { get; private init; }
    public string Cidade { get; private init; }
    public string Estado { get; private init; }
    public string Cep { get; private init; }

    // Construtor privado para o EF Core reidratar a entidade
    private EnderecoEntrega() { }

    public EnderecoEntrega(
        string logradouro,
        string numero,
        string? complemento,
        string bairro,
        string cidade,
        string estado,
        string cep)
    {
        if (string.IsNullOrWhiteSpace(logradouro))
            throw new DomainException(DomainMessages.EnderecoMSG.DadosInvalidos);

        if (string.IsNullOrWhiteSpace(numero))
            throw new DomainException(DomainMessages.EnderecoMSG.DadosInvalidos);

        if (string.IsNullOrWhiteSpace(bairro))
            throw new DomainException(DomainMessages.EnderecoMSG.DadosInvalidos);

        if (string.IsNullOrWhiteSpace(cidade))
            throw new DomainException(DomainMessages.EnderecoMSG.DadosInvalidos);

        if (string.IsNullOrWhiteSpace(estado) || estado.Trim().Length != 2)
            throw new DomainException(DomainMessages.EnderecoMSG.DadosInvalidos);

        var cepLimpo = Regex.Replace(cep ?? string.Empty, @"[^\d]", "");
        if (cepLimpo.Length != 8)
            throw new DomainException(DomainMessages.EnderecoMSG.DadosInvalidos);

        Logradouro = logradouro.Trim();
        Numero = numero.Trim();
        Complemento = complemento?.Trim();
        Bairro = bairro.Trim();
        Cidade = cidade.Trim();
        Estado = estado.Trim().ToUpper();
        Cep = cepLimpo;
    }
}