using Ecommerce.Integration.Tests.Setup;
using Ecommerce.Pedido.Api.Application.Dtos.Responses;
using EcommerceDataTest;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Ecommerce.Integration.Tests.Pedido.Integration;

[Collection("PedidoTestCollection")]
public class PedidoPatchTest : PedidoTestBase
{
    public PedidoPatchTest(PedidoWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task PatchPedido_ShouldReturnNoContent()
    {
        // Arrange
        var pedido = DataFactory.PedidoDtoCreateFaker.Generate();

        var Create = await PostAsync("/api/pedidos", pedido);
        Create.EnsureSuccessStatusCode();
        var responseCreate = await Create.Content.ReadFromJsonAsync<PedidoDtoResponse>();

        // Act
        var patch = await PatchAsync($"/api/pedidos/{responseCreate!.Id}/cancelar");
        var responsePatch = await patch.Content.ReadAsStringAsync();
        // Assert
        responsePatch.Should().BeEmpty();
        Assert.Equal(System.Net.HttpStatusCode.NoContent, patch.StatusCode);
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
    public async Task PatchPedido_ShouldReturnBadRequest_WhenPedidoAlreadyCancelled()
    {
        // Arrange
        var pedido = DataFactory.PedidoDtoCreateFaker.Generate();
        var create = await PostAsync("/api/pedidos", pedido);

        // Garante que o POST deu 201 Created ou 200 OK
        create.EnsureSuccessStatusCode();

        var responseCreate = await create.Content.ReadFromJsonAsync<PedidoDtoResponse>();

        responseCreate.Should().NotBeNull();
        responseCreate!.Id.Should().NotBeEmpty();

        // Act 1: Primeiro cancelamento (Sucesso)
        var patch1 = await PatchAsync($"/api/pedidos/{responseCreate.Id}/cancelar");

        // Assert 1
        patch1.StatusCode.Should().Be(HttpStatusCode.NoContent);
        // Act 2: Tenta cancelar novamente
        var patch2 = await PatchAsync($"/api/pedidos/{responseCreate!.Id}/cancelar");

        // Assert: O recurso existe, mas o status não permite alteração -> 400 Bad Request!
        patch2.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}