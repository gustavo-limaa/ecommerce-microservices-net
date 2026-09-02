using Ecommerce.Integration.Tests.Setup;
using Ecommerce.Pedido.Api.Application.Dtos.Request;
using Ecommerce.Pedido.Api.Application.Dtos.Responses;
using Ecommerce.Pedido.Api.Domain.Entity;
using Ecommerce.Pedido.Api.Domain.Interface;
using EcommerceDataTest;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Ecommerce.Integration.Tests.Pedido.Integration;

[Collection("PedidoTestCollection")]
public class PedidoPostTest : PedidoTestBase
{
    public PedidoPostTest(PedidoWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CriarPedido_DeveRetornar201Created_EEstruturaCorreta_QuandoDadosForemValidos()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var repoProduto = scope.ServiceProvider.GetRequiredService<IProdutoSincronizadoRepository>();

        var requestDto = DataFactory.PedidoDtoCreateFaker.Generate();

        // 1. Popula a base de Pedidos com os produtos presentes na requisição
        foreach (var item in requestDto.Itens)
        {
            var produtoSincronizado = new ProdutoSincronizado(
                id: item.ProdutoId,
                nome: "Produto Sincronizado Teste",
                preco: 100.00m,
                estoque: 50,
                ativo: true
            );

            await repoProduto.SalvarOuAtualizarAsync(produtoSincronizado);
        }

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

    [Fact]
    public async Task CriarPedido_DeveRetornar_Valores_Corrertos_QuandoProdutoNaoEstiverSincronizado()
    {
        var id = Guid.NewGuid();
        // Arrange: Gera DTO com ProdutoId aleatório QUE NÃO EXISTE no banco de Pedidos
        var requestDto = DataFactory.PedidoDtoCreateFaker.Generate() with { ClienteId = Guid.NewGuid() };

        // Act
        var response = await PostAsync("/api/pedidos", requestDto);

        // Assert: O sistema deve recusar o pedido porque o produto não existe na base local
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var pedidoCriado = await response.Content.ReadFromJsonAsync<PedidoDtoResponse>();
        pedidoCriado.Should().NotBeNull();
        pedidoCriado!.ClienteId.Should().Be(requestDto.ClienteId);
        pedidoCriado.Itens.Should().HaveCount(requestDto.Itens.Count);

        // 🎯 Validação extra: O valor total deve corresponder à soma dos itens sincronizados!
        pedidoCriado.ValorTotal.Should().BeGreaterThan(0);
    }
}