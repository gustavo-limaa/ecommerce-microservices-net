using Ecommerce.Catalogo.Api.Application.DTOs;
using Ecommerce.Integration.Tests.Setup;
using EcommerceDataTest;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace Ecommerce.Integration.Tests.Catalogo.Integration;

[Collection("CatalogoCollection")]
public class ProdutoPost : CatalogoTestBase
{
    public ProdutoPost(CatalogoWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Deve_Criar_Produto_Com_Sucesso()
    {
        // Arrange + Act
        var categoria = DataFactory.CriarCategoriaDTOFaker.Generate();
        var crreateCategoria = await PostAsync("/api/categorias", categoria);

        var res = await crreateCategoria.Content.ReadFromJsonAsync<CategoriaResponseDTO>();

        var produto = DataFactory.CriarProdutoDTOFaker(res.Id).Generate();
        var createProduto = await PostAsync("/api/produtos", produto);
        var result = await createProduto.Content.ReadFromJsonAsync<ProdutoResponseDTO>();

        // Assert
        createProduto.EnsureSuccessStatusCode();
        Assert.NotNull(result);
        Assert.Equal(produto.Nome, result.Nome);
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Categoria_Nao_Existir()
    {
        // Arrange
        var produto = DataFactory.CriarProdutoDTOFaker(Guid.NewGuid()).Generate();
        // Act
        var createProduto = await PostAsync("/api/produtos", produto);
        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, createProduto.StatusCode);
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Nome_For_Vazio()
    {
        // Arrange
        var categoria = DataFactory.CriarCategoriaDTOFaker.Generate();
        var crreateCategoria = await PostAsync("/api/categorias", categoria);
        var res = await crreateCategoria.Content.ReadFromJsonAsync<CategoriaResponseDTO>();
        var produto = DataFactory.CriarProdutoDTOFaker(res.Id).Generate();
        var novo = produto with { Nome = "" }
         ;

        // Act
        var createProduto = await PostAsync("/api/produtos", novo);
        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, createProduto.StatusCode);
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Preco_For_Negativo()
    {
        // Arrange
        var categoria = DataFactory.CriarCategoriaDTOFaker.Generate();
        var crreateCategoria = await PostAsync("/api/categorias", categoria);
        var res = await crreateCategoria.Content.ReadFromJsonAsync<CategoriaResponseDTO>();
        var produto = DataFactory.CriarProdutoDTOFaker(res.Id).Generate();
        var novo = produto with { Preco = -10.0m };
        // Act
        var createProduto = await PostAsync("/api/produtos", novo);
        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, createProduto.StatusCode);
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Estoque_For_Negativo()
    {
        // Arrange
        var categoria = DataFactory.CriarCategoriaDTOFaker.Generate();
        var crreateCategoria = await PostAsync("/api/categorias", categoria);
        var res = await crreateCategoria.Content.ReadFromJsonAsync<CategoriaResponseDTO>();
        var produto = DataFactory.CriarProdutoDTOFaker(res.Id).Generate();
        var novo = produto with { Estoque = -5 };
        // Act
        var createProduto = await PostAsync("/api/produtos", novo);
        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, createProduto.StatusCode);
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Descricao_For_Vazia()
    {
        // Arrange
        var categoria = DataFactory.CriarCategoriaDTOFaker.Generate();
        var crreateCategoria = await PostAsync("/api/categorias", categoria);
        var res = await crreateCategoria.Content.ReadFromJsonAsync<CategoriaResponseDTO>();
        var produto = DataFactory.CriarProdutoDTOFaker(res.Id).Generate();
        var novo = produto with { Descricao = "" };
        // Act
        var createProduto = await PostAsync("/api/produtos", novo);
        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, createProduto.StatusCode);
    }
}