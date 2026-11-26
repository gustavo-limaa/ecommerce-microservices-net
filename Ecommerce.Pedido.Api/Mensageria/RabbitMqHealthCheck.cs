using Ecommerce.Pedido.Api.Mensageria.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Ecommerce.Pedido.Api.Mensageria;

public class RabbitMqHealthCheck : IHealthCheck
{
    private readonly RabbitMqSettings _settings;

    public RabbitMqHealthCheck(IOptions<RabbitMqSettings> options)
    {
        _settings = options.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _settings.Host,
                Port = _settings.Port,
                UserName = _settings.Username,
                Password = _settings.Password
            };

            // Tenta criar a conexão de forma assíncrona para validar se o broker está ativo
            using var connection = await factory.CreateConnectionAsync(cancellationToken);
            using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

            return HealthCheckResult.Healthy("RabbitMQ está a responder perfeitamente.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Falha ao comunicar com o RabbitMQ.", ex);
        }
    }
}