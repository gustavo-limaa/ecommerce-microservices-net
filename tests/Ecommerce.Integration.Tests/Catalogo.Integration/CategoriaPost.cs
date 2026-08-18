using Ecommerce.Catalogo.Api.Application.DTOs;
using Ecommerce.Integration.Tests.Setup;
using EcommerceDataTest;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace Ecommerce.Integration.Tests.Catalogo.Integration;

[Collection("CatalogoCollection")]
public class CategoriaPost : CatalogoTestBase
{
    public CategoriaPost(CatalogoWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CategoriaPost_DeveRetornarSucesso()
    {
        // Arrange
        var categoria = DataFactory.CriarCategoriaDTOFaker.Generate();

        // Act
        var response = await PostAsync("/api/categorias", categoria);
        // Assert
        response.EnsureSuccessStatusCode();

        var createdCategoria = await response.Content.ReadFromJsonAsync<CategoriaResponseDTO>();

        Assert.Equal(categoria.Nome, createdCategoria!.Nome);
        Assert.Equal(categoria.Descricao, createdCategoria.Descricao);
    }

    [Fact]
    public async Task CategoriaPost_DeveRetornarErroQuandoNomeForNulo()
    {
        // Arrange
        var categoria = new CriarCategoriaDTO(null!, "Descrição de teste");
        // Act
        var response = await PostAsync("/api/categorias", categoria);
        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CategoriaPost_DeveRetornarErroQuandoNomeForVazio()
    {
        // Arrange
        var categoria = new CriarCategoriaDTO(string.Empty, "Descrição de teste");
        // Act
        var response = await PostAsync("/api/categorias", categoria);
        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CategoriaPost_DeveRetornarErroQuandoDescricaoForNula()
    {
        // Arrange
        var categoria = new CriarCategoriaDTO("Nome de teste", null!);
        // Act
        var response = await PostAsync("/api/categorias", categoria);
        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CategoriaPost_DeveRetornarErroQuandoDescricaoForVazia()
    {
        // Arrange
        var categoria = new CriarCategoriaDTO("Nome de teste", string.Empty);
        // Act
        var response = await PostAsync("/api/categorias", categoria);
        // Assert
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
}