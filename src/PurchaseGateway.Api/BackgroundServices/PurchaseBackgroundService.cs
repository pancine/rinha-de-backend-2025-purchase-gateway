
using Microsoft.Extensions.Caching.Memory;
using PurchaseGateway.Api.Models;
using PurchaseGateway.Api.Repositories;
using PurchaseGateway.Api.Services;
using System.Threading.Channels;

namespace PurchaseGateway.Api.BackgroundServices;

public class PurchaseBackgroundService : BackgroundService
{
    private readonly ChannelReader<PaymentsRequest> _paymentsReader;
    private readonly ChannelWriter<PaymentsRequest> _paymentsWriter;
    private readonly IMemoryCache _memoryCache;
    private readonly IPaymentService _defaultService;
    private readonly IPaymentService _fallbackService;
    private readonly IPurchaseRepository _repository;

    public PurchaseBackgroundService(Channel<PaymentsRequest> paymentsChannel,
        IMemoryCache memoryCache,
        [FromKeyedServices(nameof(DefaultPaymentService))] IPaymentService defaultService,
        [FromKeyedServices(nameof(FallbackPaymentService))] IPaymentService fallbackService,
        IPurchaseRepository repository)
    {
        _paymentsReader = paymentsChannel.Reader;
        _paymentsWriter = paymentsChannel.Writer;
        _memoryCache = memoryCache;
        _defaultService = defaultService;
        _fallbackService = fallbackService;
        _repository = repository;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _paymentsReader.WaitToReadAsync(stoppingToken))
        {
            var paymentsRequest = await _paymentsReader.ReadAsync(stoppingToken);

            var purchase = new Purchase
            {
                CorrelationId = paymentsRequest.CorrelationId,
                Amount = paymentsRequest.Amount,
                RequestedAt = DateTime.UtcNow
            };

            var useService = _memoryCache.Get<int>("use");
            switch (useService)
            {
                case 0:
                    if (!await _defaultService.TryProcessAsync(purchase))
                    {
                        await Task.Delay(1000, stoppingToken);
                        await _paymentsWriter.WriteAsync(paymentsRequest, stoppingToken);
                        continue;
                    }
                    break;
                case 1:
                    if (!await _fallbackService.TryProcessAsync(purchase))
                    {
                        await Task.Delay(1000, stoppingToken);
                        await _paymentsWriter.WriteAsync(paymentsRequest, stoppingToken);
                        continue;
                    }
                    break;
                default:
                    await Task.Delay(1000, stoppingToken);
                    await _paymentsWriter.WriteAsync(paymentsRequest, stoppingToken);
                    continue;
            }

            purchase.PaymentGatewayUsed = useService;
            await _repository.InsertAsync(purchase);
        }
    }
}
