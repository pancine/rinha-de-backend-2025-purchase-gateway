using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using PurchaseGateway.Api.Models;

namespace PurchaseGateway.Api;

public class HealthCheckBackgroundService(IMemoryCache cache, ILogger<HealthCheckBackgroundService> logger) : BackgroundService
{
    private readonly IMemoryCache _cache = cache;
    private readonly ILogger<HealthCheckBackgroundService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var defaultHcContent = await File.ReadAllTextAsync("/app/healthcheck/default.json", stoppingToken);
            var fallbackHcContent = await File.ReadAllTextAsync("/app/healthcheck/fallback.json", stoppingToken);

            var defaultHc = JsonConvert.DeserializeObject<HealthCheckResponse>(defaultHcContent);
            var fallbackHc = JsonConvert.DeserializeObject<HealthCheckResponse>(fallbackHcContent);

            if (!defaultHc.Failing)
            {
                _cache.Set("USE_SERVICE", PaymentGatewaysEnum.Default);
            }
            else
            {
                _cache.Set("USE_SERVICE", PaymentGatewaysEnum.Fallback);
            }

            await Task.Delay(5000, stoppingToken);
        }
    }
}
