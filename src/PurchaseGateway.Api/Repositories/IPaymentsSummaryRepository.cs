using PurchaseGateway.Api.Models;

namespace PurchaseGateway.Api.Repositories;

public interface IPaymentsSummaryRepository
{
    Task<bool> InsertRangeAsync(List<Purchase> payments);
    Task<PaymentsSummaryResponse> GetPaymentsSummaryAsync(DateTime? from = null, DateTime? to = null);
    Task PurgeAsync();
}
