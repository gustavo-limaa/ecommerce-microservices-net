namespace Ecommerce.Pedido.Api.Aplication.Dtos.Request;

public sealed record EnderecoDtoCreate(
    string Logradouro,
    string Numero,
    string complemento,
    string Bairro,
    string Cidade,
    string Estado,
    string Cep);