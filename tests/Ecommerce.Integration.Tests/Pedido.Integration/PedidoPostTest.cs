using Ecommerce.Integration.Tests.Setup;
using Ecommerce.Pedido.Api.Domain.Entity;
using Ecommerce.Pedido.Api.Domain.Interface;
using EcommerceDataTest;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace Ecommerce.Integration.Tests.Pedido.Integration;

[Collection("PedidoCollection")]
public class PedidoPostTest : PedidoTestBase
{
    public PedidoPostTest(PedidoWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task CriarPedido_DeveRetornar400BadRequest_QuandoDadosForemInvalidos()
    {
        // Arrange
        var requestDto = DataFactory.PedidoDtoCreateFaker.Generate();
        requestDto.Itens.Clear(); // Remove todos os itens para simular dados inválidos
        // Act
        var response = await PostAsync("/api/pedidos", requestDto);
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CriarPedido_DeveRetornar201Created_QuandoDadosForemValidos()
    {
        // Arrange
        var produtoId = Guid.NewGuid();
        var produtoSincronizado = new ProdutoSincronizado(produtoId, "Teclado Mecânico", 250.00m, 50, true);
        await PostAsync("api/produtossincronizados", produtoSincronizado);

        var pedidoDto = DataFactory.GerarPedidoDtoValidoComProdutos(new List<Guid> { produtoId });

        // Act
        var response = await PostAsync("api/pedidos", pedidoDto);

        // 🎯 IMPRIME A MENSAGEM DO GLOBAL EXCEPTION HANDLER CASO FALHE
        if (response.StatusCode != HttpStatusCode.Created)
        {
            var problemDetails = await response.Content.ReadAsStringAsync();
            throw new Exception($"[GlobalExceptionHandler Output]: Status {response.StatusCode} -> {problemDetails}");
        }

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }
}