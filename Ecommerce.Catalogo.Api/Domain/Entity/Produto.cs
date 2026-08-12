using Ecommerce.Catalogo.Api.Domain.GlobalErros.Exceptions;
using Ecommerce.Catalogo.Api.Domain.GlobalErros.ErrosMassege;

namespace Ecommerce.Catalogo.Api.Domain.Entity;

public sealed class Produto
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public decimal Preco { get; private set; }
    public int Estoque { get; private set; }
    public bool Ativo { get; private set; }

    public Guid CategoriaId { get; private set; }

    public Categoria Categoria { get; private set; } = null!;

    private Produto()
    { }

    public Produto(string nome, string descricao, decimal preco, int estoque, Guid categoriaId)
    {
        ValidarEDefinirDados(nome, descricao, preco, estoque, categoriaId);

        Id = Guid.NewGuid();

        Ativo = true;
    }

    public void AtualizarEstoque(int quantidade)
    {
        if (Estoque + quantidade < 0)
            throw new DomainException(ApplicationMessages.DadosInvalidos);

        Estoque += quantidade;
    }

    public void AlterarPreco(decimal novoPreco)
    {
        if (novoPreco <= 0)
            throw new DomainException(ApplicationMessages.DadosInvalidos);

        Preco = novoPreco;
    }

    public void Desativar() => Ativo = false;

    public void Ativar() => Ativo = true;

    private void ValidarEDefinirDados(string nome, string descricao, decimal preco, int estoque, Guid categoriaId)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException(ApplicationMessages.DadosInvalidos);

        if (nome.Length < 3 || nome.Length > 150)
            throw new DomainException(ApplicationMessages.DadosInvalidos);

        if (string.IsNullOrWhiteSpace(descricao))
            throw new DomainException(ApplicationMessages.DadosInvalidos);

        if (preco <= 0)
            throw new DomainException(ApplicationMessages.DadosInvalidos);

        if (estoque < 0)
            throw new DomainException(ApplicationMessages.DadosInvalidos);

        if (categoriaId == Guid.Empty)
            throw new DomainException(ApplicationMessages.DadosInvalidos);

        Nome = nome;
        Descricao = descricao;
        Preco = preco;
        Estoque = estoque;
        CategoriaId = categoriaId;
    }
}