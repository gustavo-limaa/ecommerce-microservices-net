using Ecommerce.Pedido.Api.Application.Dtos.Request;

namespace Ecommerce.Pedido.Api.Application.Dtos.Request;

public sealed record class PedidoDtoCreate(Guid ClienteId,
string CpfCliente,
EnderecoDtoCreate EnderecoEntrega,
List<ItemPedidoDtoCreate> Itens);