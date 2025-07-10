using PurchaseGateway.Api.Models;

namespace PurchaseGateway.Api.Services;

public interface IPaymentService
{
    Task ProcessAsync(PaymentRequest request);

    Task PurgeDatabaseAsync();
}
