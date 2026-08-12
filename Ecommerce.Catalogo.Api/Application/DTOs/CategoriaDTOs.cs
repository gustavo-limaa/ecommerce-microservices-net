namespace Ecommerce.Catalogo.Api.Application.DTOs;

public record CriarCategoriaDTO(string Nome, string? Descricao);

public record CategoriaResponseDTO(Guid Id, string Nome, string? Descricao, bool Ativo);