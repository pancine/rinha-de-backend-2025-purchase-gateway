using Newtonsoft.Json;
using PurchaseGateway.Models;

namespace PurchaseGateway.Services;

public class PaymentServiceBase(HttpClient httpClient) : IPaymentService
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<HealthCheckResponse> HealthCheck()
    {
        var response = await _httpClient.GetAsync("/payments/service-health");

        var content = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<HealthCheckResponse>(content) ?? new HealthCheckResponse();
    }

    public Task<bool> Process(ProcessRequest processRequest)
    {
    }
}
