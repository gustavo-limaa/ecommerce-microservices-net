using Bogus;
using Bogus.Extensions.Brazil;
using Ecommerce.Catalogo.Api.Application.DTOs;
using Ecommerce.Catalogo.Api.Domain.Entity;
using Ecommerce.Pedido.Api.Application.Dtos.Request;
using Ecommerce.Pedido.Api.Domain.Entity;
using Ecommerce.Pedido.Api.Domain.Values.Objects;

namespace EcommerceDataTest;

public static class DataFactory
{
    #region Entidade de Domínio

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

    public static Faker<ItemPedido> ItemPedidoFaker => new Faker<ItemPedido>("pt_BR")
        .CustomInstantiator(f => new ItemPedido(
            produtoId: f.Random.Guid(),
            nomeProduto: f.Commerce.ProductName(),
            precoUnitario: new ValorMonetario(f.Random.Decimal(10, 500)),
            quantidade: f.Random.Number(1, 5)
        ));

    public static Faker<Categoria> CategoriaFaker => new Faker<Categoria>("pt_BR")
        .CustomInstantiator(f => new Categoria(
            nome: f.Commerce.Department(),
            descricao: f.Commerce.ProductDescription()
        ));

    public static Faker<Produto> ProdutoFaker => new Faker<Produto>("pt_BR")
        .CustomInstantiator(f => new Produto(
            nome: f.Commerce.ProductName(),
            descricao: f.Commerce.ProductDescription(),
            preco: f.Random.Decimal(10, 500),
            estoque: f.Random.Number(1, 100),
            categoriaId: f.Random.Guid()

        ));

    #endregion Entidade de Domínio

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

    public static Faker<AtualizarEstoqueDTO> AtualizarEstoqueDTOFaker => new Faker<AtualizarEstoqueDTO>("pt_BR")
        .CustomInstantiator(f => new AtualizarEstoqueDTO(
            Quantidade: f.Random.Number(-5, 5) // Pode ser positivo ou negativo para simular adição ou subtração de estoque
        ));

    public static Faker<CriarProdutoDTO> CriarProdutoDTOFaker(Guid categoriaId) =>
    new Faker<CriarProdutoDTO>("pt_BR")
        .CustomInstantiator(f => new CriarProdutoDTO(
            Nome: f.Commerce.ProductName(),
            Descricao: f.Commerce.ProductDescription(),
            Preco: f.Random.Decimal(10, 500),
            Estoque: f.Random.Number(1, 100),
            CategoriaId: categoriaId
        ));

    public static Faker<CriarCategoriaDTO> CriarCategoriaDTOFaker => new Faker<CriarCategoriaDTO>("pt_BR")
        .CustomInstantiator(f => new CriarCategoriaDTO(
            Nome: f.Commerce.Department(),
            Descricao: f.Commerce.ProductDescription()
        ));

    public static Faker<ProdutoResponseDTO> ProdutoResponseDTOFaker => new Faker<ProdutoResponseDTO>("pt_BR")
        .CustomInstantiator(f => new ProdutoResponseDTO(
            Id: f.Random.Guid(),
            Nome: f.Commerce.ProductName(),
            Descricao: f.Commerce.ProductDescription(),
            Preco: f.Random.Decimal(10, 500),
            Estoque: f.Random.Number(1, 100),
            Ativo: f.Random.Bool(),
            CategoriaId: f.Random.Guid(),
            CategoriaNome: f.Commerce.Department()
        ));

    public static Faker<CategoriaResponseDTO> CategoriaResponseDTOFaker => new Faker<CategoriaResponseDTO>("pt_BR")
        .CustomInstantiator(f => new CategoriaResponseDTO(
            Id: f.Random.Guid(),
            Nome: f.Commerce.Department(),
            Descricao: f.Commerce.ProductDescription(),
            Ativo: f.Random.Bool()
        ));

    #endregion DTOs de Request
}