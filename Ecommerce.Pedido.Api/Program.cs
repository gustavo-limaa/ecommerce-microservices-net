using Ecommerce.Pedido.Api;
using Ecommerce.Pedido.Api.Domain.GlobalErros;
using Ecommerce.Pedido.Api.Infrastructure.Data;
using Ecommerce.Pedido.Api.Mensageria.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.OpenApi;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Configuration.AddUserSecrets<Program>();
}
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddApplication();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
// Integração do FluentValidation com o pipeline MVC para validação automática de modelos
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHostedService<ProdutoCriadoConsumer>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration.GetConnectionString("PedidoTestConnection")
    ?? "Server=127.0.0.1;Port=3308;Database=ecommerce_pedido_testes_db;Uid=root;Pwd=root;";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 30))));

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("Ecommerce - Pedido API")
            .WithTheme(ScalarTheme.Moon)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}
app.UseHttpsRedirection();

app.MapControllers();
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
}
app.Run();

internal partial class Program
{ }