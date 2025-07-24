
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
    //private readonly ChannelWriter<Purchase> _databaseWriter;

    public PurchaseBackgroundService(Channel<PaymentsRequest> paymentsChannel,
        IMemoryCache memoryCache,
        [FromKeyedServices(nameof(DefaultPaymentService))] IPaymentService defaultService,
        [FromKeyedServices(nameof(FallbackPaymentService))] IPaymentService fallbackService,
        IPurchaseRepository repository,
        Channel<Purchase> databaseChannel)
    {
        _paymentsReader = paymentsChannel.Reader;
        _paymentsWriter = paymentsChannel.Writer;
        _memoryCache = memoryCache;
        _defaultService = defaultService;
        _fallbackService = fallbackService;
        _repository = repository;
        //_databaseWriter = databaseChannel.Writer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var entitiesToInsert = new List<Purchase>(200);

        while (await _paymentsReader.WaitToReadAsync(stoppingToken))
        {
            var paymentsRequest = await _paymentsReader.ReadAsync(stoppingToken);

            var purchase = new Purchase
            {
                CorrelationId = paymentsRequest.CorrelationId,
                Amount = paymentsRequest.Amount,
                RequestedAt = DateTime.UtcNow
            };

            switch (_memoryCache.Get<int>("use"))
            {
                case 0:
                    purchase.PaymentGatewayUsed = 0;
                    await Task.WhenAll(
                        _defaultService.ProcessAsync(purchase),
                        _repository.InsertAsync(purchase)
                    );
                    break;
                case 1:
                    purchase.PaymentGatewayUsed = 1;
                    await Task.WhenAll(
                        _fallbackService.ProcessAsync(purchase),
                        _repository.InsertAsync(purchase)
                    );
                    break;
                default:
                    await Task.Delay(1000, stoppingToken);
                    await _paymentsWriter.WriteAsync(paymentsRequest, stoppingToken);
                    continue;
            }
        }
    }
}
