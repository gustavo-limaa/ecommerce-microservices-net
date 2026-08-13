using RabbitMQ.Client;

namespace Ecommerce.Catalogo.Api.Mensageria.Settings;

public class RabbitMqSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 5672;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}