using Ecommerce.Catalogo.Api.Application.DTOs;
using Ecommerce.Integration.Tests.Setup;
using EcommerceDataTest;
using FluentAssertions;
using System.Net.Http.Json;

namespace Ecommerce.Integration.Tests.Catalogo.Integration;

[Collection("CatalogoTestCollection")]
public class CategoriaGet : CatalogoTestBase
{
    public CategoriaGet(CatalogoWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetCategoriaById_ReturnsCategoria()
    {
        var dtoCriar = DataFactory.CriarCategoriaDTOFaker.Generate();
        var responsePost = await PostAsync("api/Categorias", dtoCriar);

        responsePost.EnsureSuccessStatusCode();

        var categoriaCriada = await responsePost.Content.ReadFromJsonAsync<CategoriaResponseDTO>();

        var responseGet = await GetAsync($"api/Categorias/{categoriaCriada!.Id}");

        responseGet.EnsureSuccessStatusCode();

        var categoriaRetornada = await responseGet.Content.ReadFromJsonAsync<CategoriaResponseDTO>();
        categoriaRetornada.Should().NotBeNull();
        categoriaRetornada!.Nome.Should().Be(dtoCriar.Nome);
    }

    [Fact]
    public async Task ObterTodas_RetornaListaDeCategorias()
    {
        var dtoCriar = DataFactory.CriarCategoriaDTOFaker.Generate();
        var responsePost = await PostAsync("api/Categorias", dtoCriar);
        responsePost.EnsureSuccessStatusCode();

        // 2 Act
        var responseGet = await GetAsync("api/categorias");

        // 3. Assert
        responseGet.EnsureSuccessStatusCode();

        var categorias = await responseGet.Content.ReadFromJsonAsync<List<CategoriaResponseDTO>>();
        categorias.Should().NotBeNullOrEmpty();
        categorias.Should().Contain(c => c.Nome == dtoCriar.Nome);
    }

    [Fact]
    public async Task ObterCategoriaPorId_NaoEncontrado()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        // Act
        var responseGet = await GetAsync($"api/categorias/{idInexistente}");
        // Assert
        responseGet.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ObterTodas_RetornaListaVaziaQuandoNaoExistemCategorias()
    {
        // Act+arrange
        var responseGet = await GetAsync("api/categorias");
        // Assert
        responseGet.EnsureSuccessStatusCode();
        var categorias = await responseGet.Content.ReadFromJsonAsync<List<CategoriaResponseDTO>>();
        categorias.Should().NotBeNull();
        categorias.Should().BeEmpty();
    }

    [Fact]
    public async Task ObterTodas_RetornaListaDeCategoriasComVariosItens()
    {
        // Arrange
        var dtosCriar = DataFactory.CriarCategoriaDTOFaker.Generate(5);
        foreach (var dto in dtosCriar)
        {
            var responsePost = await PostAsync("api/categorias", dto);
            responsePost.EnsureSuccessStatusCode();
        }
        // Act
        var responseGet = await GetAsync("api/categorias");
        // Assert
        responseGet.EnsureSuccessStatusCode();
        var categorias = await responseGet.Content.ReadFromJsonAsync<List<CategoriaResponseDTO>>();
        categorias.Should().NotBeNullOrEmpty();
        categorias.Should().HaveCountGreaterThanOrEqualTo(5);
    }
}