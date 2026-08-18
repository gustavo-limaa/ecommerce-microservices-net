using Ecommerce.Catalogo.Api.Domain.GlobalErros.Exceptions;
using Ecommerce.Catalogo.Api.Domain.GlobalErros.ErrosMassege;

namespace Ecommerce.Catalogo.Api.Domain.Entity;

public sealed class Categoria
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Descricao { get; private set; }
    public bool Ativo { get; private set; }

    // Construtor privado para o EF Core
    private Categoria()
    { }

    public Categoria(string nome, string? descricao)
    {
        ValidarEDefinirDados(nome, descricao);
        Id = Guid.NewGuid();

        Ativo = true;
    }

    public void Desativar() => Ativo = false;

    public void Ativar() => Ativo = true;

    public void Atualizar(string nome, string? descricao)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException(ApplicationMessages.DadosInvalidos);

        Nome = nome;
        Descricao = descricao;
    }

    private void ValidarEDefinirDados(string nome, string? descricao)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException(ApplicationMessages.DadosInvalidos);

        if (nome.Length < 2 || nome.Length > 100)
            throw new DomainException(ApplicationMessages.DadosInvalidos);
        if (descricao != null && descricao.Length > 500)
            throw new DomainException(ApplicationMessages.DadosInvalidos);
        if (descricao != null && descricao.Length < 5)
            throw new DomainException(ApplicationMessages.DadosInvalidos);
        if (descricao == null)
            throw new DomainException(ApplicationMessages.DadosInvalidos);
        Nome = nome;
        Descricao = descricao;
    }
}