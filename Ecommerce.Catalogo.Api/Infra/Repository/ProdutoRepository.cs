using Ecommerce.Catalogo.Api.Domain.Interfaces;
using global::Ecommerce.Catalogo.Api.Domain.Entity;
using global::Ecommerce.Catalogo.Api.Infra.Data;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Catalogo.Api.Infra.Repository;

public class ProdutoRepository : IProdutoRepository
{
    private readonly CatalogoDbContext _context;

    public ProdutoRepository(CatalogoDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Produto>> ObterTodosAsync() =>
        await _context.Produtos.Include(p => p.Categoria).AsNoTracking().ToListAsync();

    public async Task<Produto?> ObterPorIdAsync(Guid id) =>
        await _context.Produtos.Include(p => p.Categoria).FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IEnumerable<Produto>> ObterPorCategoriaAsync(Guid categoriaId) =>
        await _context.Produtos.Include(p => p.Categoria)
            .Where(p => p.CategoriaId == categoriaId)
            .AsNoTracking()
            .ToListAsync();

    public async Task AdicionarAsync(Produto produto)
    {
        await _context.Produtos.AddAsync(produto);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Produto produto)
    {
        _context.Produtos.Update(produto);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Produto>> ObterPaginadoAsync(int pageNumber, int pageSize)
    {
        return await _context.Produtos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}