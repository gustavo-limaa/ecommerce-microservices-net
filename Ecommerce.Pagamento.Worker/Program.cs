using Ecommerce.Pagamento.Worker;
using Ecommerce.Pagamento.Worker.Data;
using Ecommerce.Pagamento.Worker.RabbitMq;
using Ecommerce.Pagamento.Worker.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Hosting;

var builder = WebApplication.CreateBuilder(args);

// 🌐 1. Configura o Kestrel explicitamente no Worker para abrir uma porta HTTP para os Health Checks
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5004); // Podes escolher a porta ideal para o microsserviço de Pagamento
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration.GetConnectionString("PedidoTestConnection")
    ?? "Server=127.0.0.1;Port=3308;Database=ecommerce_Payment_db;Uid=root;Pwd=root;";

builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 30))));

builder.Services.AddHealthChecks()
    .AddMySql(connectionString)
    .AddCheck<RabbitMqHealthCheck>("RabbitMQ");

Console.OutputEncoding = System.Text.Encoding.UTF8;

builder.Services.AddHostedService<Worker>();
builder.Services.AddScoped<IPagamentoService, PagamentoService>();

// 💡 Transforma o Host num WebApplication para conseguir mapear endpoints HTTP
var app = builder.Build();

// Mapeia os endpoints de saúde exatamente igual às APIs
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

// Arranca o servidor web em segundo plano em simultâneo com o Worker
app.Run();