using Ecommerce.Pagamento.Worker;
using Ecommerce.Pagamento.Worker.Service;

var builder = Host.CreateApplicationBuilder(args);
Console.OutputEncoding = System.Text.Encoding.UTF8;
builder.Services.AddHostedService<Worker>();
builder.Services.AddScoped<IPagamentoService, PagamentoService>();

var host = builder.Build();
host.Run();