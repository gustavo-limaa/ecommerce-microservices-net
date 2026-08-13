using Ecommerce.Pedido.Api.Application.Service;
using Ecommerce.Pedido.Api.Domain.Interface;
using Ecommerce.Pedido.Api.Infrastructure.Data;
using Ecommerce.Pedido.Api.Infrastructure.Repositories;
using Ecommerce.Pedido.Api.Mensageria.Configuration;
using Ecommerce.Pedido.Api.Mensageria.Services;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Pedido.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(
                configuration.GetConnectionString("DefaultConnection"),
                ServerVersion.AutoDetect(configuration.GetConnectionString("DefaultConnection"))
            ));

        services.AddScoped<IPedidoRepository, PedidoRepository>();
        services.AddScoped<IEventProcessor, RabbitMqEventProcessor>();
        services.AddScoped<IProdutoSincronizadoRepository, ProdutoSincronizadoRepository>();
        services.Configure<RabbitMqSettings>(configuration.GetSection("RabbitMqSettings"));
        return services;
    }

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ServicePedido>();

        return services;
    }
}