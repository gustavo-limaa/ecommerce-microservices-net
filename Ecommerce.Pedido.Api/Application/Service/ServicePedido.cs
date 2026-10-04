using Ecommerce.Pedido.Api.Application.Dtos.Request;
using Ecommerce.Pedido.Api.Application.Dtos.Responses;
using Ecommerce.Pedido.Api.Application.Mappers.ForEntities;
using Ecommerce.Pedido.Api.Application.Mappers.ForResponse;
using Ecommerce.Pedido.Api.Domain.Common;
using Ecommerce.Pedido.Api.Domain.GlobalErros;
using Ecommerce.Pedido.Api.Domain.GlobalErros.Exceptions;
using Ecommerce.Pedido.Api.Domain.Interface;
using Ecommerce.Pedido.Api.Infrastructure.Repositories;
using Ecommerce.Pedido.Api.Mensageria.Events;
using Ecommerce.Pedido.Api.Mensageria.Services;
using Aplication = Ecommerce.Pedido.Api.Domain.GlobalErros.Exceptions.BadRequestException;

namespace Ecommerce.Pedido.Api.Application.Service;

public class ServicePedido
{
    private readonly IPedidoRepository _pedidoRepository; private readonly IProdutoSincronizadoRepository _produtoSincronizadoRepository;
    private readonly IEventProcessor _eventProcessor;

    public ServicePedido(IPedidoRepository pedidoRepository, IProdutoSincronizadoRepository produtoSincronizadoRepository, IEventProcessor eventProcessor)
    {
        _pedidoRepository = pedidoRepository;
        _produtoSincronizadoRepository = produtoSincronizadoRepository;
        _eventProcessor = eventProcessor;
    }

    public async Task<PedidoDtoResponse> AdicionarPedidoAsync(PedidoDtoCreate request, CancellationToken cancellationToken = default)
    {
        // Opcional: Validar se os produtos dos itens do pedido existem na base sincronizada
        foreach (var item in request.Itens)
        {
            var produtoSincronizado = await _produtoSincronizadoRepository.ObterPorIdAsync(item.ProdutoId, cancellationToken);
            if (produtoSincronizado is null)
            {
                throw new BadRequestException($"Produto com ID {item.ProdutoId} não foi sincronizado.");
            }
        }

        var pedido = request.ToEntity();

        await _pedidoRepository.AdicionarAsync(pedido, cancellationToken);
        var evento = new PedidoCriadoEvento(pedido.Id, pedido.ClienteId, pedido.ValorTotal.Valor, pedido.DataCriacao);
        await _eventProcessor.PublicarEventoAsync(evento, "pedido-criado-queue", cancellationToken);

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