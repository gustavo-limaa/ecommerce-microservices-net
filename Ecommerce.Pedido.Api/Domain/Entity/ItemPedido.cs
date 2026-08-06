using Ecommerce.Pedido.Api.Domain.Common;
using Ecommerce.Pedido.Api.Domain.GlobalErros;
using Ecommerce.Pedido.Api.Domain.GlobalErros.Exceptions;
using Ecommerce.Pedido.Api.Domain.Values.Objects;

namespace Ecommerce.Pedido.Api.Domain.Entity;

public sealed class ItemPedido
{
    public Guid Id { get; private set; }
    public Guid ProdutoId { get; private set; }
    public string NomeProduto { get; private set; }
    public ValorMonetario PrecoUnitario { get; private set; }
    public int Quantidade { get; private set; }

    // Propriedade calculada usando o operador de multiplicação ou cálculo direto
    public ValorMonetario ValorTotal => new(PrecoUnitario.Valor * Quantidade, PrecoUnitario.Moeda);

    // Construtor privado para o EF Core
    private ItemPedido()
    { }

    // Construtor público para criação segura no domínio
    public ItemPedido(Guid produtoId, string nomeProduto, ValorMonetario precoUnitario, int quantidade)
    {
        if (quantidade <= 0)
            throw new DomainException(DomainMessages.ItemPedidoMSG.QuantidadeInvalida);

        if (string.IsNullOrWhiteSpace(nomeProduto))
            throw new DomainException(DomainMessages.ItemPedidoMSG.NomeObrigatorio);

        Id = Guid.NewGuid();
        ProdutoId = produtoId;
        NomeProduto = nomeProduto;
        PrecoUnitario = precoUnitario;
        Quantidade = quantidade;
    }
}