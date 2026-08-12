using Ecommerce.Catalogo.Api.Application.Services;
using Ecommerce.Catalogo.Api.Domain.GlobalErros;
using Ecommerce.Catalogo.Api.Domain.Interfaces;
using Ecommerce.Catalogo.Api.Infra.Data;
using Ecommerce.Catalogo.Api.Infra.Repository;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add Controllers
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Exception Handler Padronizado (RFC 7807)
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Configuração do DbContext com MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<CatalogoDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Registros de Injeção de Dependência (DI)
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped<ICatalogoService, CatalogoService>();

var app = builder.Build();

// Middlewares
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("Ecommerce - Catalogo API")
            .WithTheme(ScalarTheme.Moon)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseAuthorization();
app.MapControllers();

app.Run();