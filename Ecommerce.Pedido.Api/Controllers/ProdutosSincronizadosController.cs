using Ecommerce.Pedido.Api.Domain.Common;
using Ecommerce.Pedido.Api.Domain.Entity;
using Ecommerce.Pedido.Api.Domain.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Pedido.Api.Controllers;

[ApiController]
[Route("api/produtossincronizados")]
public class ProdutosSincronizadosController : ControllerBase
{
    private readonly IProdutoSincronizadoRepository _repository;

    public ProdutosSincronizadosController(IProdutoSincronizadoRepository repository)
    {
        _repository = repository;
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] ProdutoSincronizado produto, CancellationToken cancellationToken)
    {
        await _repository.SalvarOuAtualizarAsync(produto, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, produto);
    }

    [HttpGet]
    public async Task<IActionResult> ObterTodos(CancellationToken cancellationToken)
    {
        var produtos = await _repository.ObterTodosAsync(cancellationToken);
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