using Ecommerce.Integration.Tests.Setup;
using Ecommerce.Pedido.Api.Application.Dtos.Responses;
using Ecommerce.Pedido.Api.Domain.Entity;
using Ecommerce.Pedido.Api.Domain.Interface;
using EcommerceDataTest;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Integration.Tests.Pedido.Integration;

[Collection("PedidoCollection")]
public class PedidoPatchTest : PedidoTestBase
{
    public PedidoPatchTest(PedidoWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task PatchPedido_ShouldReturnNotFound_WhenPedidoDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var patch = await PatchAsync($"/api/pedidos/{nonExistentId}/cancelar");

        // Assert (Após ajustar o GlobalExceptionHandler, a API devolve 404!)
        patch.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PatchPedido_ShouldReturnNoContent()
    {
        // Arrange: 1. Cadastra previamente o produto sincronizado na base
        var produtoId = Guid.NewGuid();
        var produtoSincronizado = new ProdutoSincronizado(produtoId, "Monitor Gamer 144Hz", 1200.00m, 20, true);
        await PostAsync("api/produtossincronizados", produtoSincronizado);

        // Arrange: 2. Gera o pedido vinculado ao produto existente
        var pedido = DataFactory.GerarPedidoDtoValidoComProdutos(new List<Guid> { produtoId });

        var create = await PostAsync("/api/pedidos", pedido);
        create.EnsureSuccessStatusCode();
        var responseCreate = await create.Content.ReadFromJsonAsync<PedidoDtoResponse>();

        // Act
        var patch = await PatchAsync($"/api/pedidos/{responseCreate!.Id}/cancelar");
        var responsePatch = await patch.Content.ReadAsStringAsync();

        // Assert
        responsePatch.Should().BeEmpty();
        patch.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task PatchPedido_ShouldReturnBadRequest_WhenPedidoAlreadyCancelled()
    {
        // Arrange: 1. Cadastra previamente o produto sincronizado na base
        var produtoId = Guid.NewGuid();
        var produtoSincronizado = new ProdutoSincronizado(produtoId, "Teclado Mecânico RGB", 350.00m, 15, true);
        await PostAsync("api/produtossincronizados", produtoSincronizado);

        // Arrange: 2. Gera o pedido vinculado ao produto existente
        var pedido = DataFactory.GerarPedidoDtoValidoComProdutos(new List<Guid> { produtoId });
        var create = await PostAsync("/api/pedidos", pedido);

        create.EnsureSuccessStatusCode();
        var responseCreate = await create.Content.ReadFromJsonAsync<PedidoDtoResponse>();

        responseCreate.Should().NotBeNull();
        responseCreate!.Id.Should().NotBeEmpty();

        // Act 1: Primeiro cancelamento (Sucesso -> 204 NoContent)
        var patch1 = await PatchAsync($"/api/pedidos/{responseCreate.Id}/cancelar");
        patch1.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act 2: Tenta cancelar novamente (Conflito de Regra de Negócio -> 400 ou 409)
        var patch2 = await PatchAsync($"/api/pedidos/{responseCreate.Id}/cancelar");

        // Assert
        patch2.StatusCode.Should().Be(HttpStatusCode.Conflict); // ou HttpStatusCode.Conflict conforme o GlobalExceptionHandler
    }
}