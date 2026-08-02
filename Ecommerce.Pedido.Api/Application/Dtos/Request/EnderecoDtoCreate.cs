namespace Ecommerce.Pedido.Api.Application.Dtos.Request;

public sealed record EnderecoDtoCreate(
    string Logradouro,
    string Numero,
    string complemento,
    string Bairro,
    string Cidade,
    string Estado,
    string Cep);