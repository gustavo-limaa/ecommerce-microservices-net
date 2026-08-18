using Ecommerce.Catalogo.Api.Application.DTOs;
using Ecommerce.Integration.Tests.Setup;
using EcommerceDataTest;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace Ecommerce.Integration.Tests.Catalogo.Integration;

[Collection("CatalogoCollection")]
public class ProdutoGets : CatalogoTestBase
{
    public ProdutoGets(CatalogoWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Get_Produtos_ReturnsOk()
    {
        // Arrange
        var categoria = DataFactory.CriarCategoriaDTOFaker.Generate();
        var creat = await PostAsync("/api/categorias", categoria);
        var catregoriaRetornada = await creat.Content.ReadFromJsonAsync<CategoriaResponseDTO>();

        var produtos = DataFactory.CriarProdutoDTOFaker(catregoriaRetornada.Id).Generate(5);
        foreach (var p in produtos)
        {
            var responseCreate = await PostAsync("/api/produtos", p);
            responseCreate.EnsureSuccessStatusCode();
        }

        // Act
        var response = await GetAsync("/api/produtos");
        // Assert
        var produtosRetornados = await response.Content.ReadFromJsonAsync<List<ProdutoResponseDTO>>();
        response.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        produtosRetornados.Should().HaveCountGreaterThanOrEqualTo(5);
    }

    [Fact]
    public async Task Get_ProdutoById_ReturnsOk()
    {
        // Arrange
        var categoria = DataFactory.CriarCategoriaDTOFaker.Generate();
        var creat = await PostAsync("/api/categorias", categoria);
        var catregoriaRetornada = await creat.Content.ReadFromJsonAsync<CategoriaResponseDTO>();
        var produto = DataFactory.CriarProdutoDTOFaker(catregoriaRetornada.Id).Generate();
        var responseCreate = await PostAsync("/api/produtos", produto);
        responseCreate.EnsureSuccessStatusCode();
        var produtoCriado = await responseCreate.Content.ReadFromJsonAsync<ProdutoResponseDTO>();
        // Act
        var response = await GetAsync($"/api/produtos/{produtoCriado.Id}");
        // Assert
        var produtoRetornado = await response.Content.ReadFromJsonAsync<ProdutoResponseDTO>();
        response.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        produtoRetornado.Should().NotBeNull();
        produtoRetornado.Id.Should().Be(produtoCriado.Id);
    }

    [Fact]
    public async Task Get_ProdutoById_ReturnsNotFound()
    {
        // Arrange
        var produtoIdInexistente = Guid.NewGuid();
        // Act
        var response = await GetAsync($"/api/produtos/{produtoIdInexistente}");
        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_Produtos_thereisNoProducts_ReturnsEmptyList()
    {
        // Arrange
        // Ensure the database is empty or clear it before this test if necessary
        // Act
        var response = await GetAsync("/api/produtos");
        // Assert
        var produtosRetornados = await response.Content.ReadFromJsonAsync<List<ProdutoResponseDTO>>();
        response.EnsureSuccessStatusCode();
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        produtosRetornados.Should().BeEmpty();
    }

    [Fact]
    public async Task Get_Produtos_ReturnsPaginatedResults()
    {
        // Arrange
        var categoria = DataFactory.CriarCategoriaDTOFaker.Generate();
        var creat = await PostAsync("/api/categorias", categoria);
        var categoriaRetornada = await creat.Content.ReadFromJsonAsync<CategoriaResponseDTO>();

        // Gera 20 produtos vinculados à categoria criada
        var produtos = DataFactory.CriarProdutoDTOFaker(categoriaRetornada!.Id).Generate(20);

        foreach (var p in produtos)
        {
            var responseCreate = await PostAsync("/api/produtos", p);
            responseCreate.EnsureSuccessStatusCode();
        }

        // Act: Solicita apenas a página 1 com tamanho 10
        var response = await GetAsync("/api/produtos?pageNumber=1&pageSize=10");

        // Assert
        response.EnsureSuccessStatusCode();

        var produtosRetornados = await response.Content.ReadFromJsonAsync<List<ProdutoResponseDTO>>();

        // Confirma que a página 1 cortou a lista no tamanho solicitado (10 itens)
        produtosRetornados.Should().NotBeNull();
        produtosRetornados.Should().HaveCount(10);
    }
}