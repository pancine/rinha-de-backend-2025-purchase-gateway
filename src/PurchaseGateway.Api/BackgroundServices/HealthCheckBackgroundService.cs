
using Microsoft.Extensions.Caching.Memory;
using PurchaseGateway.Api.Services;
using StackExchange.Redis;

namespace PurchaseGateway.Api.BackgroundServices;

public class HealthCheckBackgroundService : BackgroundService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDatabase _redis;
    private readonly IPaymentService _defaultService;
    private readonly IPaymentService _fallbackService;
    private readonly int LockValue = Random.Shared.Next();
    public HealthCheckBackgroundService(IMemoryCache memoryCache,
        IConnectionMultiplexer mutex,
        [FromKeyedServices(nameof(DefaultPaymentService))] IPaymentService defaultService,
        [FromKeyedServices(nameof(FallbackPaymentService))] IPaymentService fallbackService)
    {
        _memoryCache = memoryCache;
        _redis = mutex.GetDatabase();
        _defaultService = defaultService;
        _fallbackService = fallbackService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _memoryCache.Set("use", -1);
        await SetAndGetHealthCheckAsync(stoppingToken);

        using PeriodicTimer timer = new(TimeSpan.FromSeconds(5));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await SetAndGetHealthCheckAsync(stoppingToken);
        }
    }

    private async Task SetAndGetHealthCheckAsync(CancellationToken stoppingToken)
    {
        if (await _redis.LockTakeAsync("hc", LockValue, TimeSpan.FromSeconds(2)))
        {
            var results = await Task.WhenAll(
                _defaultService.HealthCheckAsync(),
                _fallbackService.HealthCheckAsync()
            );

            if (!results[0].Failing && results[0].MinResponseTime / 4 <= results[1].MinResponseTime)
            {
                await _redis.StringSetAsync("use", 0);
                _memoryCache.Set("use", 0);
            }
            else if (!results[1].Failing)
            {
                await _redis.StringSetAsync("use", 1);
                _memoryCache.Set("use", 1);
            }
            else
            {
                await _redis.StringSetAsync("use", -1);
                _memoryCache.Set("use", 2);
            }

            return;
        }

        while (true)
        {
            if (await _redis.LockTakeAsync("hc", LockValue, TimeSpan.FromSeconds(1)))
            {
                (await _redis.StringGetAsync("use")).TryParse(out int useService);
                _memoryCache.Set("use", useService);

                await _redis.LockReleaseAsync("hc", LockValue);
                return;
            }

            await Task.Delay(500, stoppingToken);
        }
    }
}
