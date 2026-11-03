using Ecommerce.Pedido.Api;
using Ecommerce.Pedido.Api.Domain.GlobalErros;
using Ecommerce.Pedido.Api.Infrastructure.Data;
using Ecommerce.Pedido.Api.Mensageria.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

using FluentValidation.AspNetCore;
using Ecommerce.Pedido.Api.Domain.GlobalErros.Middleware;

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
app.UseMiddleware<CorrelationIdMiddleware>();
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

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeaderName = "X-Correlation-Id";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICorrelationIdContext correlationContext)
    {
        if (!context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var headerValues) ||
            !Guid.TryParse(headerValues.FirstOrDefault(), out var correlationId))
        {
            correlationId = Guid.NewGuid();
        }

        correlationContext.SetCorrelationId(correlationId);

        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(CorrelationIdHeaderName))
            {
                context.Response.Headers.Append(CorrelationIdHeaderName, correlationId.ToString());
            }
            return Task.CompletedTask;
        });

        await _next(context);
    }
}