using Ecommerce.Catalogo.Api.Application.DTOs;
using Ecommerce.Integration.Tests.Setup;
using EcommerceDataTest;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace Ecommerce.Integration.Tests.Catalogo.Integration;

[Collection("CatalogoCollection")]
public class ProdutoPatch : CatalogoTestBase
{
    public ProdutoPatch(CatalogoWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Quantidade_For_Invalida_Ou_Negativa()
    {
        // Arrange
        var categoria = DataFactory.CriarCategoriaDTOFaker.Generate();
        var createCategoria = await PostAsync("/api/categorias", categoria);
        var res = await createCategoria.Content.ReadFromJsonAsync<CategoriaResponseDTO>();

        var produto = DataFactory.CriarProdutoDTOFaker(res!.Id).Generate();
        var createProduto = await PostAsync("/api/produtos", produto);
        var result = await createProduto.Content.ReadFromJsonAsync<ProdutoResponseDTO>();

        var estoqueInvalido = new AtualizarEstoqueDTO(0);

        // Act
        var patchResponse = await PatchAsync($"/api/produtos/{result!.Id}/estoque", JsonContent.Create(estoqueInvalido));

        patchResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Produto_Nao_Existir()
    {
        // Arrange
        var estoqueovo = DataFactory.AtualizarEstoqueDTOFaker.Generate();
        var produtoIdInexistente = Guid.NewGuid();
        // Act
        var patchResponse = await PatchAsync($"/api/produtos/{produtoIdInexistente}/estoque", JsonContent.Create(estoqueovo));
        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, patchResponse.StatusCode);
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Quantidade_For_Negativa()
    {
        // Arrange
        var categoria = DataFactory.CriarCategoriaDTOFaker.Generate();
        var crreateCategoria = await PostAsync("/api/categorias", categoria);
        var res = await crreateCategoria.Content.ReadFromJsonAsync<CategoriaResponseDTO>();
        var produto = DataFactory.CriarProdutoDTOFaker(res!.Id).Generate();
        var createProduto = await PostAsync("/api/produtos", produto);
        var result = await createProduto.Content.ReadFromJsonAsync<ProdutoResponseDTO>();
        // Act
        var estoqueovoNegativo = new AtualizarEstoqueDTO(-5);
        var patchResponse = await PatchAsync($"/api/produtos/{result!.Id}/estoque", JsonContent.Create(estoqueovoNegativo));
        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, patchResponse.StatusCode);
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Quantidade_For_Zero()
    {
        // Arrange
        var categoria = DataFactory.CriarCategoriaDTOFaker.Generate();
        var crreateCategoria = await PostAsync("/api/categorias", categoria);
        var res = await crreateCategoria.Content.ReadFromJsonAsync<CategoriaResponseDTO>();
        var produto = DataFactory.CriarProdutoDTOFaker(res!.Id).Generate();
        var createProduto = await PostAsync("/api/produtos", produto);
        var result = await createProduto.Content.ReadFromJsonAsync<ProdutoResponseDTO>();
        // Act
        var estoqueovoZero = new AtualizarEstoqueDTO(0);
        var patchResponse = await PatchAsync($"/api/produtos/{result!.Id}/estoque", JsonContent.Create(estoqueovoZero));
        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, patchResponse.StatusCode);
    }
}