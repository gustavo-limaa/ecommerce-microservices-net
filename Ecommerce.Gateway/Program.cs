using Ecommerce.Gateway;
using Ecommerce.Gateway.Extension;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGatewayRateLimiter();
builder.Services.AddGatewayAuthentication(builder.Configuration);
builder.Services.AddGatewayResilience();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseMiddleware<GatewayCorrelationIdMiddleware>();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();

app.Run();

namespace Ecommerce.Gateway
{
    public partial class Program
    { }
}

public class GatewayCorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeaderName = "X-Correlation-Id";

    public GatewayCorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Se a requisição chegou sem o ID (ex: do front-end), o Gateway gera um.
        if (!context.Request.Headers.ContainsKey(CorrelationIdHeaderName))
        {
            // Adiciona o header na requisição de entrada. O YARP reencaminha isto automaticamente para as APIs.
            context.Request.Headers.Append(CorrelationIdHeaderName, Guid.NewGuid().ToString());
        }

        await _next(context);
    }
}