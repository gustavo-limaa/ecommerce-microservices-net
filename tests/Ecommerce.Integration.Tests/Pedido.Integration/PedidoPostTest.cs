using Ecommerce.Integration.Tests.Setup;
using Ecommerce.Pedido.Api.Application.Dtos.Request;
using Ecommerce.Pedido.Api.Application.Dtos.Responses;
using EcommerceDataTest;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Ecommerce.Integration.Tests.Pedido.Integration;

[Collection("Integration Tests")]
public class PedidoPostTest : TestBase
{
    public PedidoPostTest(PedidoWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CriarPedido_DeveRetornar201Created_EEstruturaCorreta_QuandoDadosForemValidos()
    {
        // Arrange
        var requestDto = DataFactory.PedidoDtoCreateFaker.Generate();
        // Act
        var response = await PostAsync("/api/pedidos", requestDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var pedidoCriado = await response.Content.ReadFromJsonAsync<PedidoDtoResponse>();
        pedidoCriado.Should().NotBeNull();
        pedidoCriado!.ClienteId.Should().Be(requestDto.ClienteId);
        pedidoCriado.Itens.Should().HaveCount(requestDto.Itens.Count);
    }

    [Fact]
    public async Task CriarPedido_DeveRetornar400BadRequest_QuandoCpfForInvalido()
    {
        // Arrange
        var requestDto = DataFactory.PedidoDtoCreateFaker.Generate() with { CpfCliente = "123.456.789-00" };

        // Act
        var response = await PostAsync("/api/pedidos", requestDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CriarPedido_DeveRetornar400BadRequest_QuandoEnderecoForNulo()
    {
        // Arrange
        var requestDto = DataFactory.PedidoDtoCreateFaker.Generate() with { EnderecoEntrega = null! };

        // Act
        var response = await PostAsync("/api/pedidos", requestDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CriarPedido_DeveRetornar400BadRequest_QuandoItensForemNulos()
    {
        // Arrange
        var requestDto = DataFactory.PedidoDtoCreateFaker.Generate() with { Itens = null! };

        // Act
        var response = await PostAsync("/api/pedidos", requestDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CriarPedido_DeveRetornar400BadRequest_QuandoClienteIdForVazio()
    {
        // Arrange
        var requestDto = DataFactory.PedidoDtoCreateFaker.Generate() with { ClienteId = Guid.Empty };

        // Act
        var response = await PostAsync("/api/pedidos", requestDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CriarPedido_DeveRetornar400BadRequest_QuandoListaDeItensForVazia()
    {
        // Arrange
        var requestDto = DataFactory.PedidoDtoCreateFaker.Generate() with { Itens = new List<ItemPedidoDtoCreate>() };

        // Act
        var response = await PostAsync("/api/pedidos", requestDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}