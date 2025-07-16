using Microsoft.Extensions.Caching.Memory;
using PurchaseGateway.Api.Models;
using PurchaseGateway.Api.Services;
using System.Threading.Channels;

namespace PurchaseGateway.Api;

public class PurchaseBackgroundService(
    Channel<PaymentRequest> channel,
    Channel<Purchase> databaseChannel,
    IMemoryCache cache,
    [FromKeyedServices(PaymentGatewaysEnum.Default)] IPaymentService defaultPaymentService,
    [FromKeyedServices(PaymentGatewaysEnum.Fallback)] IPaymentService fallbackPaymentService) : BackgroundService
{
    private readonly ChannelReader<PaymentRequest> _reader = channel.Reader;
    private readonly ChannelWriter<Purchase> _databaseChannel = databaseChannel.Writer;

    private readonly IMemoryCache _cache = cache;
    private readonly IPaymentService _defaultPaymentService = defaultPaymentService;
    private readonly IPaymentService _fallbackPaymentService = fallbackPaymentService;

    private readonly ParallelOptions ParallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = -1 };

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _reader.WaitToReadAsync(stoppingToken))
        {
            var requests = _reader.ReadAllAsync(stoppingToken);

            await Parallel.ForEachAsync(requests, ParallelOptions, async (request, stoppingToken) =>
            {
                PaymentGatewaysEnum service;

                while (!_cache.TryGetValue("USE_SERVICE", out service)) { await Task.Delay(1000, stoppingToken); }

                if (service == PaymentGatewaysEnum.Default)
                {
                    if (await _defaultPaymentService.ProcessAsync(request))
                    {
                        await _databaseChannel.WriteAsync(new Purchase()
                        {
                            Amount = request.Amount,
                            RequestedAt = DateTime.UtcNow,
                            PaymentGatewayUsed = 0
                        }, stoppingToken);
                    }
                    return;
                }

                await _fallbackPaymentService.ProcessAsync(request);
                await _databaseChannel.WriteAsync(new Purchase()
                {
                    Amount = request.Amount,
                    RequestedAt = DateTime.UtcNow,
                    PaymentGatewayUsed = 1
                }, stoppingToken);
            });
        }
    }
}
