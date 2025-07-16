using PurchaseGateway.Api.Models;

namespace PurchaseGateway.Api.Services;

public interface IPaymentService
{
    Task<bool> ProcessAsync(PaymentRequest request);
    Task PurgeDatabaseAsync();
}
