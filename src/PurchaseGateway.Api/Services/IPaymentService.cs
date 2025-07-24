using PurchaseGateway.Api.Models;

namespace PurchaseGateway.Api.Services;

public interface IPaymentService
{
    Task<HealthCheckResponse> HealthCheckAsync();
    Task<bool> ProcessAsync(Purchase purchase);

    Task PurgeDatabaseAsync();
}
