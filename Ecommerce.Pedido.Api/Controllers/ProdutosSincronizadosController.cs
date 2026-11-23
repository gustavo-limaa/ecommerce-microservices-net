using Ecommerce.Pedido.Api.Domain.Common;
using Ecommerce.Pedido.Api.Domain.Entity;
using Ecommerce.Pedido.Api.Domain.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Ecommerce.Pedido.Api.Controllers;

[ApiController]
[Route("api/produtossincronizados")]
public class ProdutosSincronizadosController : ControllerBase
{
    private const string cacheKey = "produtos_sincronizados:todos";
    private readonly IProdutoSincronizadoRepository _repository;
    private readonly IMemoryCache _memoryCache;

    public ProdutosSincronizadosController(IProdutoSincronizadoRepository repository, IMemoryCache memoryCache)
    {
        _repository = repository;
        _memoryCache = memoryCache;
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] ProdutoSincronizado produto, CancellationToken cancellationToken)
    {
        await _repository.SalvarOuAtualizarAsync(produto, cancellationToken);
        _memoryCache.Remove(cacheKey);

        return StatusCode(StatusCodes.Status201Created, produto);
    }

    [HttpGet]
    public async Task<IActionResult> ObterTodos(CancellationToken cancellationToken)
    {
        var produtos = await _memoryCache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
            return await _repository.ObterTodosAsync(cancellationToken);
        });

        return Ok(produtos);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var produto = await _repository.ObterPorIdAsync(id, cancellationToken);

        if (produto is null)
            return NotFound(ApplicationMessages.NaoEncontrado);

        return Ok(produto);
    }
}