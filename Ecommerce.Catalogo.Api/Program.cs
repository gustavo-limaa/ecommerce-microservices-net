using Ecommerce.Catalogo.Api.Application.Services;
using Ecommerce.Catalogo.Api.Domain.GlobalErros;
using Ecommerce.Catalogo.Api.Domain.Interfaces;
using Ecommerce.Catalogo.Api.Infra.Data;
using Ecommerce.Catalogo.Api.Infra.Repository;
using Ecommerce.Catalogo.Api.Mensageria.Services;
using Ecommerce.Catalogo.Api.Mensageria.Settings;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers & OpenAPI
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Global Exception Handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Mensageria (RabbitMQ)
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMqSettings"));
builder.Services.AddScoped<IEventProcessor, RabbitMqEventProcessor>();

// Database Context (EF Core + MySQL com versão fixa)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<CatalogoDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 30))));

// Injeção de Dependências de Negócio
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