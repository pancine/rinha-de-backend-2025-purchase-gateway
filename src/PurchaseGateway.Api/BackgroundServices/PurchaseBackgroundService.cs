using PurchaseGateway.Api.Models;
using PurchaseGateway.Api.Repositories;
using PurchaseGateway.Api.Services;
using System.Threading.Channels;

namespace PurchaseGateway.Api.BackgroundServices;

public class PurchaseBackgroundService : BackgroundService
{
    private readonly ChannelReader<PaymentsRequest> _paymentsReader;
    private readonly ChannelWriter<PaymentsRequest> _paymentsWriter;
    private readonly IPaymentService _defaultService;
    private readonly IPaymentService _fallbackService;
    private readonly IPurchaseRepository _repository;

    public PurchaseBackgroundService(Channel<PaymentsRequest> paymentsChannel,
        [FromKeyedServices(nameof(DefaultPaymentService))] IPaymentService defaultService,
        [FromKeyedServices(nameof(FallbackPaymentService))] IPaymentService fallbackService,
        IPurchaseRepository repository)
    {
        _paymentsReader = paymentsChannel.Reader;
        _paymentsWriter = paymentsChannel.Writer;
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

            if (await _defaultService.TryProcessAsync(purchase))
            {
                purchase.PaymentGatewayUsed = 0;
                await _repository.InsertAsync(purchase);
                continue;
            }

            if (await _fallbackService.TryProcessAsync(purchase))
            {
                purchase.PaymentGatewayUsed = 1;
                await _repository.InsertAsync(purchase);
                continue;
            }

            await Task.Delay(1000, stoppingToken);
            await _paymentsWriter.WriteAsync(paymentsRequest, stoppingToken);
        }
    }
}
