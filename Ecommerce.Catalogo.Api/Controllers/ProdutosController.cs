using Ecommerce.Catalogo.Api.Application.DTOs;
using Ecommerce.Catalogo.Api.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Catalogo.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProdutosController : ControllerBase
    {
        private readonly ICatalogoService _catalogoService;

        public ProdutosController(ICatalogoService catalogoService)
        {
            _catalogoService = catalogoService;
        }

        [HttpGet]
        public async Task<IActionResult> ObterTodos()
        {
            var produtos = await _catalogoService.ObterProdutosAsync();
            return Ok(produtos);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> ObterPorId(Guid id)
        {
            var produto = await _catalogoService.ObterProdutoPorIdAsync(id);
            return Ok(produto);
        }

        [HttpPost]
        public async Task<IActionResult> Criar([FromBody] CriarProdutoDTO dto)
        {
            var produto = await _catalogoService.CriarProdutoAsync(dto);
            return CreatedAtAction(nameof(ObterPorId), new { id = produto.Id }, produto);
        }

        [HttpPatch("{id:guid}/estoque")]
        public async Task<IActionResult> AtualizarEstoque(Guid id, [FromBody] AtualizarEstoqueDTO dto)
        {
            await _catalogoService.AtualizarEstoqueAsync(id, dto.Quantidade);
            return NoContent();
        }
    }
}