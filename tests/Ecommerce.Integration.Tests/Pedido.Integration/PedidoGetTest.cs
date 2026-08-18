using Ecommerce.Integration.Tests.Setup;
using Ecommerce.Pedido.Api.Application.Dtos.Responses;
using EcommerceDataTest;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace Ecommerce.Integration.Tests.Pedido.Integration;

[Collection("PedidoTestCollection")]
public class PedidoGetTest : PedidoTestBase
{
    public PedidoGetTest(PedidoWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ObterPedido_DeveRetornar200Ok_EEstruturaCorreta_QuandoPedidoExistir()
    {
        // Arrange (Gera o DTO válido para o POST)
        var requestDto = DataFactory.PedidoDtoCreateFaker.Generate();

        var responsePost = await PostAsync("/api/pedidos", requestDto);
        responsePost.EnsureSuccessStatusCode();
        responsePost.StatusCode.Should().Be(HttpStatusCode.Created);

        var pedidoCriado = await responsePost.Content.ReadFromJsonAsync<PedidoDtoResponse>();
        pedidoCriado.Should().NotBeNull();

        // Act
        var response = await GetAsync($"/api/pedidos/{pedidoCriado!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var pedidoObtido = await response.Content.ReadFromJsonAsync<PedidoDtoResponse>();
        pedidoObtido.Should().NotBeNull();
        pedidoObtido!.Id.Should().Be(pedidoCriado.Id);
        pedidoObtido.ClienteId.Should().Be(requestDto.ClienteId);
    }

    [Fact]
    public async Task ObterPedido_DeveRetornar404NotFound_QuandoPedidoNaoExistir()
    {
        // Arrange

        // Act
        var response = await GetAsync($"/api/pedidos/{Guid.NewGuid()}");
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ObterPedido_DeveRetornar200listVazia_QuandoPedidoExistir()
    {
        // Arrange

        // Act
        var response = await GetAsync($"/api/pedidos/");
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ObterPedidos_DeveRetornar200Ok_EListaComQuantidadeCorreta_QuandoExistiremPedidos()
    {
        // Arrange (Gera 4 DTOs e cadastra na API)
        var requestsDto = DataFactory.PedidoDtoCreateFaker.Generate(4);

        foreach (var dto in requestsDto)
        {
            var responsePost = await PostAsync("/api/pedidos", dto);
            responsePost.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        // Act (Busca todos os pedidos cadastrados)
        var response = await GetAsync("/api/pedidos/");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Desserializa como LISTA
        var pedidosObtidos = await response.Content.ReadFromJsonAsync<List<PedidoDtoResponse>>();

        pedidosObtidos.Should().NotBeNull();

        // 🎯 Validação do Count no FluentAssertions:
        pedidosObtidos.Should().HaveCount(4);
    }
}