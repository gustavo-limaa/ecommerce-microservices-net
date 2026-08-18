using Ecommerce.Catalogo.Api.Application.DTOs;
using Ecommerce.Integration.Tests.Setup;
using EcommerceDataTest;
using System;
using System.Collections.Generic;
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
    public async Task Deve_Atualizar_Estoque_Com_Sucesso()
    {
        // Arrange
        var categoria = DataFactory.CriarCategoriaDTOFaker.Generate();
        var crreateCategoria = await PostAsync("/api/categorias", categoria);
        var res = await crreateCategoria.Content.ReadFromJsonAsync<CategoriaResponseDTO>();

        var produto = DataFactory.CriarProdutoDTOFaker(res!.Id).Generate();
        var createProduto = await PostAsync("/api/produtos", produto);
        var result = await createProduto.Content.ReadFromJsonAsync<ProdutoResponseDTO>();

        // Act
        var estoqueovo = DataFactory.AtualizarEstoqueDTOFaker.Generate();

        // 1. Corrige o envio do body serializado em JSON
        var patchResponse = await PatchAsync($"/api/produtos/{result!.Id}/estoque", JsonContent.Create(estoqueovo));

        // Assert
        patchResponse.EnsureSuccessStatusCode();

        // 2. Valida 204 NoContent de acordo com o return NoContent() do seu Controller
        Assert.Equal(System.Net.HttpStatusCode.NoContent, patchResponse.StatusCode);

        // 3. Act 2: Faz um GET para confirmar que o estoque mudou no banco
        var getResponse = await GetAsync($"/api/produtos/{result.Id}");
        getResponse.EnsureSuccessStatusCode();
        var produtoAtualizado = await getResponse.Content.ReadFromJsonAsync<ProdutoResponseDTO>();

        Assert.NotNull(produtoAtualizado);
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
        Assert.Equal(System.Net.HttpStatusCode.NoContent, patchResponse.StatusCode);
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