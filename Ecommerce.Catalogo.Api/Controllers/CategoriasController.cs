using Ecommerce.Catalogo.Api.Application.DTOs;
using Ecommerce.Catalogo.Api.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Ecommerce.Catalogo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private const string cacheKey = "categorias:todas";
    private readonly ICatalogoService _catalogoService;
    private readonly IMemoryCache _memoryCache;

    public CategoriasController(ICatalogoService catalogoService, IMemoryCache memoryCache)
    {
        _catalogoService = catalogoService;
        _memoryCache = memoryCache;
    }

    [HttpGet]
    public async Task<IActionResult> ObterTodas()
    {
        var categorias = await _memoryCache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
            return await _catalogoService.ObterCategoriasAsync();
        });

        return Ok(categorias);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var categoria = await _catalogoService.ObterCategoriaPorIdAsync(id);
        return Ok(categoria);
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarCategoriaDTO dto)
    {
        var categoria = await _catalogoService.CriarCategoriaAsync(dto);
        _memoryCache.Remove(cacheKey);

        return CreatedAtAction(nameof(ObterPorId), new { id = categoria.Id }, categoria);
    }
}