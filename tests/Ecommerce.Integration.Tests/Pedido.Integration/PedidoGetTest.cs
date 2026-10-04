using Ecommerce.Integration.Tests.Setup;
using Ecommerce.Pedido.Api.Application.Dtos.Request;
using Ecommerce.Pedido.Api.Application.Dtos.Responses;
using Ecommerce.Pedido.Api.Domain.Entity;
using EcommerceDataTest;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace Ecommerce.Integration.Tests.Pedido.Integration;

[Collection("PedidoCollection")]
public class PedidoGetTest : PedidoTestBase
{
    public PedidoGetTest(PedidoWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ObterPedido_DeveRetornar200Ok_EEstruturaCorreta_QuandoPedidoExistir()
    {
        // Arrange: 1. Sincroniza o produto
        var produtoId = Guid.NewGuid();
        var produtoSincronizado = new ProdutoSincronizado(produtoId, "Produto Teste", 100m, 10, true);
        await PostAsync("api/produtossincronizados", produtoSincronizado);

        // Arrange: 2. Gera o pedido associado ao produto cadastrado
        var requestDto = DataFactory.GerarPedidoDtoValidoComProdutos(new List<Guid> { produtoId });

        var responsePost = await PostAsync("/api/pedidos", requestDto);
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
        // 1. ARRANGE: Cadastra o produto sincronizado no banco de teste
        var produtoId = Guid.NewGuid();
        var produtoSincronizado = new ProdutoSincronizado(produtoId, "Teclado Mecânico", 250.00m, 50, true);
        await PostAsync("api/produtossincronizados", produtoSincronizado);

        // 2. Prepara 4 DTOs apontando para esse produto que existe no banco
        var requestsDto = DataFactory.PedidoDtoCreateFaker.Generate(4);
        foreach (var dto in requestsDto)
        {
            dto.Itens.Clear();
            dto.Itens.Add(new ItemPedidoDtoCreate(produtoId, "Teclado Mecânico", 1, 250.00m));

            var responsePost = await PostAsync("/api/pedidos", dto);
            responsePost.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        // 3. ACT: Busca os pedidos
        var response = await GetAsync("/api/pedidos/");

        // 4. ASSERT
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var pedidosObtidos = await response.Content.ReadFromJsonAsync<List<PedidoDtoResponse>>();
        pedidosObtidos.Should().NotBeNull();
        pedidosObtidos.Should().HaveCount(4);
    }
}