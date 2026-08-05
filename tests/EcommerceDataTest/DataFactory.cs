using Bogus;
using Bogus.Extensions.Brazil;
using Ecommerce.Pedido.Api.Application.Dtos.Request;
using Ecommerce.Pedido.Api.Domain.Entity;
using Ecommerce.Pedido.Api.Domain.Values.Objects;

namespace EcommerceDataTest;

public static class DataFactory
{
    // --- Entidade de Domínio ---
    public static Faker<Pedido> PedidoFaker => new Faker<Pedido>("pt_BR")
        .CustomInstantiator(f => new Pedido(
            clienteId: f.Random.Guid(),
            cpfCliente: new ObjectCPF(f.Person.Cpf(false)),
            enderecoEntrega: new EnderecoEntrega(
                logradouro: f.Address.StreetName(),
                numero: f.Address.BuildingNumber(),
                complemento: f.Address.SecondaryAddress(),
                bairro: f.Address.County(),
                cidade: f.Address.City(),
                estado: f.Address.StateAbbr(),
                cep: f.Address.ZipCode("#####-###")
            )
        ));

    public static Pedido GerarPedidoValidoComItens(int quantidadeItens = 2)
    {
        var pedido = PedidoFaker.Generate();

        for (int i = 0; i < quantidadeItens; i++)
        {
            var item = new ItemPedido(
                produtoId: Guid.NewGuid(),
                nomeProduto: new Faker("pt_BR").Commerce.ProductName(),
                precoUnitario: new ValorMonetario(new Faker().Random.Decimal(10, 500)),
                quantidade: new Faker().Random.Number(1, 5)
            );

            pedido.AdicionarItem(item);
        }

        return pedido;
    }

    #region DTOs de Request

    public static Faker<PedidoDtoCreate> PedidoDtoCreateFaker => new Faker<PedidoDtoCreate>("pt_BR")
        .CustomInstantiator(f => new PedidoDtoCreate(
            ClienteId: f.Random.Guid(),
            CpfCliente: f.Person.Cpf(false),
            EnderecoEntrega: EnderecoDtoCreateFaker.Generate(),
            Itens: ItemPedidoDtoCreateFaker.Generate(2) // 2 itens costuma ser ideal para testes rápidos
        ));

    public static Faker<EnderecoDtoCreate> EnderecoDtoCreateFaker => new Faker<EnderecoDtoCreate>("pt_BR")
        .CustomInstantiator(f => new EnderecoDtoCreate(
            Logradouro: f.Address.StreetName(),
            Numero: f.Address.BuildingNumber(),
            complemento: f.Address.SecondaryAddress(),
            Bairro: f.Address.County(),
            Cidade: f.Address.City(),
            Estado: f.Address.StateAbbr(),
            Cep: f.Address.ZipCode("#####-###")
        ));

    public static Faker<ItemPedidoDtoCreate> ItemPedidoDtoCreateFaker => new Faker<ItemPedidoDtoCreate>("pt_BR")
        .CustomInstantiator(f => new ItemPedidoDtoCreate(
            ProdutoId: f.Random.Guid(),
            NomeProduto: f.Commerce.ProductName(),
            PrecoUnitario: f.Random.Decimal(10, 500),
            Quantidade: f.Random.Number(1, 5)
        ));

    #endregion DTOs de Request
}