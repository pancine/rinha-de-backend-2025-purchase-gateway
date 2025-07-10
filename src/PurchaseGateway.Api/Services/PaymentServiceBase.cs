using Newtonsoft.Json;
using PurchaseGateway.Api.Models;
using System.Net.Http.Headers;

namespace PurchaseGateway.Api.Services;

public class PaymentServiceBase : IPaymentService
{
    protected readonly HttpClient _httpClient = new HttpClient();

    public async Task<PaymentsSummary> GetPaymentsSummaryAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "admin/payments-summary");
        request.Headers.Add("X-Rinha-Token", "123");

        var response = await _httpClient.SendAsync(request);

        return JsonConvert.DeserializeObject<PaymentsSummary>(await response.Content.ReadAsStringAsync());
    }

    public Task ProcessAsync(PaymentRequest request)
    {
        request.RequestedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
        return _httpClient.PostAsync("payments",
            new StringContent(JsonConvert.SerializeObject(request), new MediaTypeHeaderValue("application/json"))
        );
    }

    public Task PurgeDatabaseAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "admin/purge-payments");
        request.Headers.Add("X-Rinha-Token", "123");

        return _httpClient.SendAsync(request);
    }
}
