namespace Ecommerce.Auth.Api.Dtos;

public sealed record TokenResponseDto(string Token, int Expiracao, string Email);