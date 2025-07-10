using PurchaseGateway.Api.Models;

namespace PurchaseGateway.Api.Services;

public interface IPaymentService
{
    Task<PaymentsSummary> GetPaymentsSummaryAsync();
    Task ProcessAsync(PaymentRequest request);
    Task PurgeDatabaseAsync();
}
