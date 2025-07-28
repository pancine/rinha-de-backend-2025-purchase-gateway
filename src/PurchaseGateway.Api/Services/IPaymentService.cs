using PurchaseGateway.Api.Models;

namespace PurchaseGateway.Api.Services;

public interface IPaymentService
{
    Task<bool> TryProcessAsync(Purchase purchase);

    Task PurgeDatabaseAsync();
}
