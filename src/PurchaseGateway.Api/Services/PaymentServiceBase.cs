using Newtonsoft.Json;
using PurchaseGateway.Api.Models;
using System.Net.Http.Headers;

namespace PurchaseGateway.Api.Services;

public class PaymentServiceBase : IPaymentService
{
    protected readonly HttpClient _httpClient = new HttpClient();

    public async Task<bool> ProcessAsync(PaymentRequest request)
    {
        request.RequestedAt = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
        var response = await _httpClient.PostAsync("payments",
            new StringContent(JsonConvert.SerializeObject(request), new MediaTypeHeaderValue("application/json"))
        );

        return response.IsSuccessStatusCode;
    }

    public Task PurgeDatabaseAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "admin/purge-payments");
        request.Headers.Add("X-Rinha-Token", "123");

        return _httpClient.SendAsync(request);
    }
}
