using Ecommerce.Pedido.Api;
using Ecommerce.Pedido.Api.Domain.GlobalErros;
using Ecommerce.Pedido.Api.Mensageria.Services;
using FluentValidation;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services
    .AddInfrastructure(builder.Configuration)
    .AddApplication();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHostedService<ProdutoCriadoConsumer>();
var app = builder.Build();
app.UseExceptionHandler();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
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

app.Run();