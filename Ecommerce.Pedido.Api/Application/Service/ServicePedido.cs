using Ecommerce.Pedido.Api.Application.Dtos.Request;
using Ecommerce.Pedido.Api.Application.Dtos.Responses;
using Ecommerce.Pedido.Api.Application.Mappers.ForEntities;
using Ecommerce.Pedido.Api.Application.Mappers.ForResponse;
using Ecommerce.Pedido.Api.Domain.Common;
using Ecommerce.Pedido.Api.Domain.GlobalErros;
using Ecommerce.Pedido.Api.Domain.GlobalErros.Exceptions;
using Ecommerce.Pedido.Api.Domain.Interface;
using Aplication = Ecommerce.Pedido.Api.Domain.GlobalErros.Exceptions.BadRequestException;

namespace Ecommerce.Pedido.Api.Application.Service;

public class ServicePedido
{
    private readonly IPedidoRepository _pedidoRepository;

    public ServicePedido(IPedidoRepository pedidoRepository)
    {
        _pedidoRepository = pedidoRepository;
    }

    public async Task<PedidoDtoResponse> AdicionarPedidoAsync(PedidoDtoCreate request, CancellationToken cancellationToken = default)
    {
        var pedido = request.ToEntity();

        await _pedidoRepository.AdicionarAsync(pedido, cancellationToken);

        return pedido.ToResponse();
    }

    public async Task<PedidoDtoResponse?> ObterPedidoPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var pedido = await _pedidoRepository.ObterPorIdAsync(id, cancellationToken);

        return pedido?.ToResponse();
    }

    public async Task<IEnumerable<PedidoDtoResponse>> ObterTodosPedidosAsync(CancellationToken cancellationToken = default)
    {
        var pedidos = await _pedidoRepository.ObterTodosAsync(cancellationToken);

        return pedidos.Select(p => p.ToResponse());
    }

    public async Task<IEnumerable<PedidoDtoResponse>> ObterPedidosComFiltroAsync(
        StatusPedido? status,
        int pagina = 1,
        int tamanhoPagina = 10,
        CancellationToken cancellationToken = default)
    {
        var pedidos = await _pedidoRepository.ObterComFiltroAsync(status, pagina, tamanhoPagina, cancellationToken);

        return pedidos.Select(p => p.ToResponse());
    }

    public async Task CancelarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var pedido = await _pedidoRepository.ObterPorIdAsync(id, cancellationToken);
        if (Guid.Empty.Equals(id))
            throw new NotFoundException(ApplicationMessages.Pedido.NaoEncontrado);

        if (pedido is null)
            throw new NotFoundException(ApplicationMessages.Pedido.NaoEncontrado);
        if (pedido.Status == StatusPedido.Cancelado)
            throw new ConflictException(ApplicationMessages.Pedido.StatusInvalidoParaAtualizacao);
        if (pedido.Status != StatusPedido.Processando)
            throw new BadRequestException(ApplicationMessages.Pedido.StatusInvalidoParaAtualizacao);

        pedido.Cancelar();

        await _pedidoRepository.AtualizarAsync(pedido, cancellationToken);
    }
}