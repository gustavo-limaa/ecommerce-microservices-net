using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Ecommerce.Gateway.Extension;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddGatewayAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var secretKey = configuration["JwtSettings:Secret"]
            ?? throw new InvalidOperationException("A chave JWT 'JwtSettings:Secret' não foi configurada nas variáveis de ambiente!");

        var key = Encoding.ASCII.GetBytes(secretKey);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };
        });

        // Configuração da política 'Authenticated' que o YARP exige
        services.AddAuthorization(options =>
        {
            options.AddPolicy("Authenticated", policy =>
                policy.RequireAuthenticatedUser());
        });

        return services;
    }
}