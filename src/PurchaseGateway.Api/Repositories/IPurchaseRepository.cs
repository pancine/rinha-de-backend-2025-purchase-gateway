using PurchaseGateway.Api.Models;

namespace PurchaseGateway.Api.Repositories;

public interface IPurchaseRepository
{
    Task<bool> InsertAsync(Purchase payment);
    Task InsertRangeAsync(List<Purchase> payments);
    Task<PaymentsSummaryResponse> GetPaymentsSummaryAsync(DateTime? from = null, DateTime? to = null);
    Task PurgeAsync();
}
