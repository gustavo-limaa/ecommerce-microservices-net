using Ecommerce.Catalogo.Api.Domain.Interfaces;
using global::Ecommerce.Catalogo.Api.Domain.Entity;
using global::Ecommerce.Catalogo.Api.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Catalogo.Api.Infra.Repository;

public class CategoriaRepository : ICategoriaRepository
{
    private readonly CatalogoDbContext _context;

    public CategoriaRepository(CatalogoDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Categoria>> ObterTodasAsync() =>
        await _context.Categorias.AsNoTracking().ToListAsync();

    public async Task<Categoria?> ObterPorIdAsync(Guid id) =>
        await _context.Categorias.FirstOrDefaultAsync(c => c.Id == id);

    public async Task AdicionarAsync(Categoria categoria)
    {
        await _context.Categorias.AddAsync(categoria);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Categoria categoria)
    {
        _context.Categorias.Update(categoria);
        await _context.SaveChangesAsync();
    }
}