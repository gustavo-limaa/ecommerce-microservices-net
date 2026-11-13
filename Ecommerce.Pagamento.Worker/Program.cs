using Ecommerce.Pagamento.Worker;
using Ecommerce.Pagamento.Worker.Data;
using Ecommerce.Pagamento.Worker.Service;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration.GetConnectionString("PedidoTestConnection")
    ?? "Server=127.0.0.1;Port=3308;Database=ecommerce_Payment_db;Uid=root;Pwd=root;";

builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 30))));

Console.OutputEncoding = System.Text.Encoding.UTF8;
builder.Services.AddHostedService<Worker>();
builder.Services.AddScoped<IPagamentoService, PagamentoService>();
builder.Services.AddMemoryCache();

var host = builder.Build();
host.Run();