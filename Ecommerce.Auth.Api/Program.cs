using Ecommerce.Auth.Api.Data;
using Ecommerce.Auth.Api.Endpoints;
using Ecommerce.Auth.Api.Service;
using Ecommerce.Auth.Api.Services;
using Microsoft.EntityFrameworkCore;

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

var app = builder.Build();

app.MapAuthEndpoints();

app.Run();

public partial class Program
{ }