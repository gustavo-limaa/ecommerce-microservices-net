using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace Ecommerce.Gateway.Extension;

public static class ResilienceExtensions
{
    public static IServiceCollection AddGatewayResilience(this IServiceCollection services)
    {
        services.AddHttpClient("YarpHttpClient")
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.MaxRetryAttempts = 3;
                options.Retry.Delay = TimeSpan.FromMilliseconds(500);
                options.Retry.BackoffType = DelayBackoffType.Exponential;

                options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
            });

        return services;
    }
}