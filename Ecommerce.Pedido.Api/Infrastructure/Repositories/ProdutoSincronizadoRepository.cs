using Ecommerce.Pedido.Api.Domain.Entity;
using Ecommerce.Pedido.Api.Domain.Interface;
using Ecommerce.Pedido.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Pedido.Api.Infrastructure.Repositories;

public class ProdutoSincronizadoRepository : IProdutoSincronizadoRepository
{
    private readonly AppDbContext _context;

    public ProdutoSincronizadoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task SalvarOuAtualizarAsync(ProdutoSincronizado produto, CancellationToken cancellationToken = default)
    {
        var produtoExistente = await _context.ProdutosSincronizados
            .FirstOrDefaultAsync(p => p.Id == produto.Id, cancellationToken);

        if (produtoExistente is null)
        {
            await _context.ProdutosSincronizados.AddAsync(produto, cancellationToken);
        }
        else
        {
            produtoExistente.Nome = produto.Nome;
            produtoExistente.Preco = produto.Preco;
            produtoExistente.Estoque = produto.Estoque;
            produtoExistente.Ativo = produto.Ativo;
            _context.ProdutosSincronizados.Update(produtoExistente);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ProdutoSincronizado?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ProdutosSincronizados
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<ProdutoSincronizado>> ObterTodosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ProdutosSincronizados
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}