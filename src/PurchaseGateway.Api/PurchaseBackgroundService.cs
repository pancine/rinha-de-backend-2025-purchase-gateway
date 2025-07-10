using Microsoft.Extensions.Caching.Memory;
using PurchaseGateway.Api.Models;
using PurchaseGateway.Api.Services;
using System.Threading.Channels;

namespace PurchaseGateway.Api;

public class PurchaseBackgroundService(
    Channel<PaymentRequest> channel,
    IMemoryCache cache,
    [FromKeyedServices(PaymentGatewaysEnum.Default)] IPaymentService defaultPaymentService,
    [FromKeyedServices(PaymentGatewaysEnum.Fallback)] IPaymentService fallbackPaymentService) : BackgroundService
{
    private readonly ChannelReader<PaymentRequest> _reader = channel.Reader;

    private readonly IMemoryCache _cache = cache;
    private readonly IPaymentService _defaultPaymentService = defaultPaymentService;
    private readonly IPaymentService _fallbackPaymentService = fallbackPaymentService;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _reader.WaitToReadAsync(stoppingToken))
        {
            var request = await _reader.ReadAsync(stoppingToken);

            if ((PaymentGatewaysEnum)_cache.Get("USE_SERVICE") == PaymentGatewaysEnum.Default)
            {
                await _defaultPaymentService.ProcessAsync(request);
                continue;
            }
            await _fallbackPaymentService.ProcessAsync(request);
        }
    }
}
