using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using PurchaseGateway.Api.Models;

namespace PurchaseGateway.Api;

public class HealthCheckBackgroundService(IMemoryCache cache) : BackgroundService
{
    private readonly IMemoryCache _cache = cache;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var results = await Task.WhenAll(
                File.ReadAllTextAsync("/app/healthcheck/default.json", stoppingToken),
                File.ReadAllTextAsync("/app/healthcheck/fallback.json", stoppingToken)
            );

            var defaultHc = JsonConvert.DeserializeObject<HealthCheckResponse>(results[0])!;
            var fallbackHc = JsonConvert.DeserializeObject<HealthCheckResponse>(results[1])!;

            if (!defaultHc.Failing && defaultHc.MinResponseTime / 4 < fallbackHc.MinResponseTime)
            {
                _cache.Set("USE_SERVICE", PaymentGatewaysEnum.Default);
            }
            else if (!fallbackHc.Failing)
            {
                _cache.Set("USE_SERVICE", PaymentGatewaysEnum.Fallback);
            }
            else
            {
                _cache.Remove("USE_SERVICE");
            }

            await Task.Delay(5000, stoppingToken);
        }
    }
}
