using PurchaseGateway.Api.Models;
using PurchaseGateway.Api.Repositories;
using System.Threading.Channels;

namespace PurchaseGateway.Api;

public class PaymentsSummaryBackgroundService(Channel<Purchase> channel, IPaymentsSummaryRepository paymentsRepository) : BackgroundService
{
    private readonly ChannelReader<Purchase> _reader = channel.Reader;

    private readonly IPaymentsSummaryRepository _paymentsRepository = paymentsRepository;
    private readonly List<Purchase> _paymentsToInsert = new();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var purchases = _reader.ReadAllAsync(stoppingToken);

            await foreach (var purchase in purchases)
            {
                _paymentsToInsert.Add(purchase);

                if (_paymentsToInsert.Count > 10)
                {
                    await _paymentsRepository.InsertRangeAsync(_paymentsToInsert);
                    _paymentsToInsert.Clear();
                }
            }
        }
    }
}
