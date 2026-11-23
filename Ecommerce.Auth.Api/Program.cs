using Ecommerce.Auth.Api.Data;
using Ecommerce.Auth.Api.Endpoints;
using Ecommerce.Auth.Api.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string não encontrada.");

builder.Services.AddDbContext<UsuarioDbContext>(options =>
{
    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 0, 36)) // Substitui o AutoDetect
    );
});
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddHealthChecks()
    .AddMySql(connectionString);
var jwtSecret = builder.Configuration["JwtSettings:Secret"]
    ?? throw new InvalidOperationException("JwtSettings:Secret não configurado!");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        }; options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"JWT Falhou: {context.Exception.Message}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapAuthEndpoints();
using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<UsuarioDbContext>();
context.Database.Migrate();

app.UseAuthentication();
app.UseAuthorization();

app.Run();

namespace Ecommerce.Auth.Api
{
    public partial class Program
    { }
}