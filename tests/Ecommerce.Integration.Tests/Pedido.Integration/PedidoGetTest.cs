using Ecommerce.Integration.Tests.Setup;
using Ecommerce.Pedido.Api.Application.Dtos.Responses;
using EcommerceDataTest;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace Ecommerce.Integration.Tests.Pedido.Integration
{
    public class PedidoGetTest : TestBase
    {
        public PedidoGetTest(PedidoWebApplicationFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task ObterPedido_DeveRetornar200Ok_EEstruturaCorreta_QuandoPedidoExistir()
        {
            // Arrange
            var pedido = DataFactory.PedidoFaker.Generate();
            // Act
            var response = await GetAsync($"/api/pedidos/{pedido.Id}");
            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            pedido.Should().NotBeNull();
            pedido!.Id.Should().Be(pedido.Id);
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
    }
}