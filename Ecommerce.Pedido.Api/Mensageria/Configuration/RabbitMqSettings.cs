using RabbitMQ.Client;

namespace Ecommerce.Pedido.Api.Mensageria.Configuration;

public class RabbitMqSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}