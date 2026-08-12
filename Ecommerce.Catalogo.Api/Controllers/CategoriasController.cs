using Ecommerce.Catalogo.Api.Application.DTOs;
using Ecommerce.Catalogo.Api.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Catalogo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly ICatalogoService _catalogoService;

    public CategoriasController(ICatalogoService catalogoService)
    {
        _catalogoService = catalogoService;
    }

    [HttpGet]
    public async Task<IActionResult> ObterTodas()
    {
        var categorias = await _catalogoService.ObterCategoriasAsync();
        return Ok(categorias);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarCategoriaDTO dto)
    {
        var categoria = await _catalogoService.CriarCategoriaAsync(dto);
        return CreatedAtAction(nameof(ObterTodas), new { id = categoria.Id }, categoria);
    }
}