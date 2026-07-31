using Ecommerce.Pedido.Api.Domain.Common;
using Ecommerce.Pedido.Api.Domain.Values.Objects;
using global::Ecommerce.Pedido.Api.Domain.GlobalErros;

namespace Ecommerce.Pedido.Api.Domain.Entity;

public sealed class Pedido
{
    public Guid Id { get; private set; }
    public Guid ClienteId { get; private set; }
    public ObjectCPF CpfCliente { get; private set; }
    public DateTime DataCriacao { get; private set; }
    public StatusPedido Status { get; private set; }
    public EnderecoEntrega EnderecoEntrega { get; private set; }

    // Encapsulamento da lista de itens
    private readonly List<ItemPedido> _itens = new();

    public IReadOnlyCollection<ItemPedido> Itens => _itens.AsReadOnly();

    // Propriedade calculada do valor total dinâmico do pedido
    public ValorMonetario ValorTotal =>
        new(_itens.Sum(item => item.ValorTotal.Valor));

    // Construtor privado para o EF Core
    private Pedido()
    { }

    // Construtor de Domínio
    public Pedido(Guid clienteId, ObjectCPF
         cpfCliente, EnderecoEntrega enderecoEntrega)
    {
        if (clienteId == Guid.Empty)
            throw new DomainException(DomainMessages.PedidoMSG.ClienteInvalido);

        Id = Guid.NewGuid();
        ClienteId = clienteId;
        CpfCliente = cpfCliente ?? throw new DomainException(DomainMessages.CpfMSG.Obrigatorio);
        EnderecoEntrega = enderecoEntrega ?? throw new DomainException(DomainMessages.PedidoMSG.EnderecoObrigatorio);

        DataCriacao = DateTime.UtcNow;
        Status = StatusPedido.Processando;
    }

    // --- Métodos de Comportamento do Domínio ---

    public void AdicionarItem(ItemPedido item)
    {
        if (item is null)
            throw new DomainException(DomainMessages.PedidoMSG.PedidoInvalido);

        if (Status != StatusPedido.Processando)
            throw new DomainException(DomainMessages.PedidoMSG.AlteracaoNaoPermitida);

        _itens.Add(item);
    }

    public void AlterarStatus(StatusPedido novoStatus)
    {
        // Validação de transição de status usando Switch Expression
        Status = (Status, novoStatus) switch
        {
            // Regra: Do 'Processando', só pode ir para 'Aprovado' ou 'Reprovado'
            (StatusPedido.Processando, StatusPedido.Aprovado) => StatusPedido.Aprovado,
            (StatusPedido.Processando, StatusPedido.Reprovado) => StatusPedido.Reprovado,

            // Regra: Do 'Aprovado', só pode ir para 'ACaminho'
            (StatusPedido.Aprovado, StatusPedido.ACaminho) => StatusPedido.ACaminho,

            // Qualquer outra combinação não permitida dispara exceção de domínio!
            _ => throw new DomainException($"Transição inválida de status: de {Status} para {novoStatus}.")
        };
    }
}