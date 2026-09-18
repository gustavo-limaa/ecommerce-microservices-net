using Ecommerce.Auth.Api.Entities;
using Ecommerce.Auth.Api.Service;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Ecommerce.Auth.Api.Services;

public class TokenService(IConfiguration configuration) : ITokenService
{
    public string GerarToken(Usuario usuario)
    {
        var secretKey = configuration["JwtSettings:Secret"]
            ?? throw new InvalidOperationException("JwtSettings:Secret não configurado!");

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(secretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome),\
                new Claim(ClaimTypes.Email, usuario.Email.Valor),
                new Claim(ClaimTypes.Role, usuario.Perfil.ToString())
            ]),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}