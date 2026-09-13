using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Ecommerce.Gateway.Extension;

public static class RateLimiterExtensions
{
    public static IServiceCollection AddGatewayRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("IpRateLimit", context =>
            {
                var clientIp = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                               ?? context.Connection.RemoteIpAddress?.ToString()
                               ?? "client-global-key";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: clientIp,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,                 // Baixamos para 5 para você ver o bloqueio mais rápido!
                        Window = TimeSpan.FromSeconds(10),
                        QueueLimit = 0
                    });
            });
        });

        return services;
    }
}