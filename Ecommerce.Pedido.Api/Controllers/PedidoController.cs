using Ecommerce.Pedido.Api.Application.Dtos.Request;
using Ecommerce.Pedido.Api.Application.Dtos.Responses;
using Ecommerce.Pedido.Api.Application.Service;
using Ecommerce.Pedido.Api.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Ecommerce.Pedido.Api.Controllers;

[ApiController]
[Route("api/pedidos")]
public class PedidosController : ControllerBase
{
    private readonly ServicePedido _pedidoService;
    private readonly IMemoryCache _memoryCache;

    public PedidosController(ServicePedido pedidoService, IMemoryCache memoryCache)
    {
        _pedidoService = pedidoService;
        _memoryCache = memoryCache;
    }

    [HttpPost]
    public async Task<ActionResult<PedidoDtoResponse>> Criar([FromBody] PedidoDtoCreate request, CancellationToken cancellationToken)
    {
        var response = await _pedidoService.AdicionarPedidoAsync(request, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = response.Id }, response);
    }

    [HttpGet("{id:guid}", Name = "ObterPedidoPorId")]
    public async Task<ActionResult<PedidoDtoResponse>> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var response = await _pedidoService.ObterPedidoPorIdAsync(id, cancellationToken);

        if (response is null)
            return NotFound(new { mensagem = ApplicationMessages.Pedido.NaoEncontrado });

        return Ok(response);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PedidoDtoResponse>>> ObterComFiltro(
        [FromQuery] StatusPedido? status,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 10,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"pedidos:status:{status}:pagina:{pagina}:tamanhoPagina:{tamanhoPagina}";
        var response = await _memoryCache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
            return await _pedidoService.ObterPedidosComFiltroAsync(status, pagina, tamanhoPagina, cancellationToken);
        });
        return Ok(response);
    }

    [HttpPatch("{id:guid}/cancelar")]
    public async Task<IActionResult> Cancelar(Guid id, CancellationToken cancellationToken)
    {
        await _pedidoService.CancelarAsync(id, cancellationToken);
        return NoContent();
    }
}