using PurchaseGateway.Models;

namespace PurchaseGateway.Services;

public interface IPaymentService
{
    Task<HealthCheckResponse> HealthCheck();
    Task<bool> Process(ProcessRequest processRequest);
}
