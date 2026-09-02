using Ecommerce.Pedido.Api.Domain.Common;
using Ecommerce.Pedido.Api.Domain.GlobalErros.Exceptions;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Pedido.Api.Domain.Entity;

// Entidade local no banco do Pedido
public class ProdutoSincronizado
{
    // Construtor completo para criação/sincronização via evento
    public ProdutoSincronizado(Guid id, string nome, decimal preco, int estoque, bool ativo = true)
    {
        if (id == Guid.Empty)
            throw new DomainException(ApplicationMessages.DadosInvalidos);

        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException(ApplicationMessages.DadosInvalidos);

        if (preco < 0)
            throw new DomainException(ApplicationMessages.DadosInvalidos);

        if (estoque < 0)
            throw new DomainException(ApplicationMessages.DadosInvalidos);

        Id = id;
        Nome = nome;
        Preco = preco;
        Estoque = estoque;
        Ativo = ativo;
    }

    public Guid Id { get; internal set; }
    public string Nome { get; internal set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Preco { get; internal set; }

    public int Estoque { get; internal set; }
    public bool Ativo { get; internal set; }

    // Construtor sem parâmetros para o Entity Framework
    protected ProdutoSincronizado()
    { }

    // Método para atualizar informações em um eventual reprocessamento do evento
    public void AtualizarDados(string nome, decimal preco, int estoque, bool ativo)
    {
        Nome = nome;
        Preco = preco;
        Estoque = estoque;
        Ativo = ativo;
    }
}