using Ecommerce.Pedido.Api.Domain.Values.Objects;

namespace Ecommerce.Pedido.Api.Aplication.Dtos.Request;

public sealed record class PedidoDtoCreate(Guid ClienteId,
ObjectCPF CpfCliente,
EnderecoDtoCreate EnderecoEntrega,
List<ItemPedidoDtoCreate> Itens);