using Ecommerce.Catalogo.Api.Application.DTOs;
using Ecommerce.Catalogo.Api.Domain.Entity;
using Ecommerce.Catalogo.Api.Domain.Interfaces;
using Ecommerce.Catalogo.Api.Mensageria.Events;
using Ecommerce.Catalogo.Api.Mensageria.Services;
using Ecommerce.Integration.Tests.Setup;
using EcommerceDataTest;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace Ecommerce.Integration.Tests.Catalogo.Integration.Rabbitqm;

[Collection("CatalogoCollection")]
public class CatalogoProcessadorTesteEvent : CatalogoTestBase
{
    public CatalogoProcessadorTesteEvent(CatalogoWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Deve_Publicar_Normalmente()
    {
        // 1. Arrange: Configura o Mock do EventProcessor
        using var scope = Factory.Services.CreateScope();
        var eventProcessorMock = scope.ServiceProvider.GetRequiredService<Mock<IEventProcessor>>();

        eventProcessorMock
            .Setup(x => x.PublicarEventoAsync(
                It.IsAny<ProdutoCriadoEvent>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // 2. Persiste a Categoria diretamente no banco para garantir que ela exista
        var categoriaRepo = scope.ServiceProvider.GetRequiredService<ICategoriaRepository>();
        var categoria = new Categoria("Categoria Teste Evento", "Descrição Válida");
        await categoriaRepo.AdicionarAsync(categoria);

        // Prepara o DTO do Produto vinculado à Categoria criada
        var produtoDto = DataFactory.CriarProdutoDTOFaker(categoria.Id).Generate();

        // 3. Limpa qualquer histórico de chamada anterior
        eventProcessorMock.Invocations.Clear();

        // 4. Act: Cria o produto via HTTP
        var response = await PostAsync("/api/produtos", produtoDto);

        // 5. Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        eventProcessorMock.Verify(x => x.PublicarEventoAsync(
            It.IsAny<ProdutoCriadoEvent>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Deve_Retornar_Erro_Quando_Falhar_Ao_Publicar_Evento_No_RabbitMQ()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var eventProcessorMock = scope.ServiceProvider.GetRequiredService<Mock<IEventProcessor>>();

        eventProcessorMock
            .Setup(x => x.PublicarEventoAsync(
                It.IsAny<ProdutoCriadoEvent>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Erro de conexão com o broker RabbitMQ."));

        var categoria = DataFactory.CriarCategoriaDTOFaker.Generate();
        var createCategoria = await PostAsync("/api/categorias", categoria);
        var res = await createCategoria.Content.ReadFromJsonAsync<CategoriaResponseDTO>();

        var produto = DataFactory.CriarProdutoDTOFaker(res!.Id).Generate();

        var response = await PostAsync("/api/produtos", produto);

        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        eventProcessorMock.Verify(x => x.PublicarEventoAsync(
            It.IsAny<ProdutoCriadoEvent>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}