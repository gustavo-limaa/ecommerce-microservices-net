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

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("A ConnectionString 'DefaultConnection' não foi configurada.");
builder.Services.AddDbContext<CatalogoDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 30))));
// Injeção de Dependências de Negócio
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped<ICatalogoService, CatalogoService>();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CatalogoDbContext>(); // Substitua pelo nome do seu DbContext do Catálogo
    context.Database.Migrate();
}
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

internal partial class Program

{ }