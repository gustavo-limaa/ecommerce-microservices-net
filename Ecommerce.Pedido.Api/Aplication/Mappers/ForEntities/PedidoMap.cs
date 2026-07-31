using Ecommerce.Pedido.Api.Aplication.Dtos.Request;
using Ecommerce.Pedido.Api.Domain.Entity;
using Ecommerce.Pedido.Api.Domain.Values.Objects;
using PedidoEntity = Ecommerce.Pedido.Api.Domain.Entity.Pedido;

namespace Ecommerce.Pedido.Api.Application.Mappers.ForEntities;

public static class PedidoCreateMapper
{
    public static PedidoEntity ToEntity(this PedidoDtoCreate request)
    {
        var cpf = new ObjectCPF(request.CpfCliente.Valor);

        var endereco = new EnderecoEntrega(
            request.EnderecoEntrega.Logradouro,
            request.EnderecoEntrega.Numero,
            request.EnderecoEntrega.complemento,
            request.EnderecoEntrega.Bairro,
            request.EnderecoEntrega.Cidade,
            request.EnderecoEntrega.Estado,
            request.EnderecoEntrega.Cep
        );

        var pedido = new PedidoEntity(request.ClienteId, cpf, endereco);

        foreach (var itemDto in request.Itens)
        {
            var item = new ItemPedido(
                itemDto.ProdutoId,
                itemDto.NomeProduto,
                new ValorMonetario(itemDto.PrecoUnitario),
                itemDto.Quantidade
            );

            pedido.AdicionarItem(item);
        }

        return pedido;
    }
}