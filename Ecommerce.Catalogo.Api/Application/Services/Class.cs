using Ecommerce.Catalogo.Api.Application.DTOs;
using Ecommerce.Catalogo.Api.Domain.GlobalErros.ErrosMassege;
using Ecommerce.Catalogo.Api.Domain.Interfaces;
using global::Ecommerce.Catalogo.Api.Domain.Entity;
using global::Ecommerce.Catalogo.Api.Domain.GlobalErros.Exceptions;

namespace Ecommerce.Catalogo.Api.Application.Services;

public class CatalogoService : ICatalogoService
{
    private readonly IProdutoRepository _produtoRepository;
    private readonly ICategoriaRepository _categoriaRepository;

    public CatalogoService(IProdutoRepository produtoRepository, ICategoriaRepository categoriaRepository)
    {
        _produtoRepository = produtoRepository;
        _categoriaRepository = categoriaRepository;
    }

    public async Task<IEnumerable<CategoriaResponseDTO>> ObterCategoriasAsync()
    {
        var categorias = await _categoriaRepository.ObterTodasAsync();
        return categorias.Select(c => new CategoriaResponseDTO(c.Id, c.Nome, c.Descricao, c.Ativo));
    }

    public async Task<CategoriaResponseDTO> CriarCategoriaAsync(CriarCategoriaDTO dto)
    {
        var categoria = new Categoria(dto.Nome, dto.Descricao);
        await _categoriaRepository.AdicionarAsync(categoria);

        return new CategoriaResponseDTO(categoria.Id, categoria.Nome, categoria.Descricao, categoria.Ativo);
    }

    public async Task<IEnumerable<ProdutoResponseDTO>> ObterProdutosAsync()
    {
        var produtos = await _produtoRepository.ObterTodosAsync();
        return produtos.Select(p => new ProdutoResponseDTO(
            p.Id, p.Nome, p.Descricao, p.Preco, p.Estoque, p.Ativo, p.CategoriaId, p.Categoria?.Nome ?? string.Empty
        ));
    }

    public async Task<ProdutoResponseDTO> ObterProdutoPorIdAsync(Guid id)
    {
        var produto = await _produtoRepository.ObterPorIdAsync(id)
            ?? throw new NotFoundException(ApplicationMessages.NaoEncontrado);

        return new ProdutoResponseDTO(
            produto.Id, produto.Nome, produto.Descricao, produto.Preco, produto.Estoque, produto.Ativo, produto.CategoriaId, produto.Categoria?.Nome ?? string.Empty
        );
    }

    public async Task<ProdutoResponseDTO> CriarProdutoAsync(CriarProdutoDTO dto)
    {
        var categoria = await _categoriaRepository.ObterPorIdAsync(dto.CategoriaId)
            ?? throw new NotFoundException(ApplicationMessages.NaoEncontrado);

        var produto = new Produto(dto.Nome, dto.Descricao, dto.Preco, dto.Estoque, dto.CategoriaId);
        await _produtoRepository.AdicionarAsync(produto);

        return new ProdutoResponseDTO(
            produto.Id, produto.Nome, produto.Descricao, produto.Preco, produto.Estoque, produto.Ativo, categoria.Id, categoria.Nome
        );
    }

    public async Task AtualizarEstoqueAsync(Guid id, int quantidade)
    {
        var produto = await _produtoRepository.ObterPorIdAsync(id)
            ?? throw new NotFoundException(ApplicationMessages.NaoEncontrado);

        produto.AtualizarEstoque(quantidade);
        await _produtoRepository.AtualizarAsync(produto);
    }
}