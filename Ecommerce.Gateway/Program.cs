using Ecommerce.Gateway.Extension;

var builder = WebApplication.CreateBuilder(args);

// 1. REGISTRO DE SERVIÇOS NA INJEÇÃO DE DEPENDÊNCIA (DI)
builder.Services.AddGatewayRateLimiter();
builder.Services.AddGatewayAuthentication(builder.Configuration);
builder.Services.AddGatewayResilience();

// 2. CONFIGURAÇÃO DO YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// 3. MIDDLEWARES DA APLICAÇÃO
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();

app.Run();