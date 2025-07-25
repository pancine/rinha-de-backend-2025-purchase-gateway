using Newtonsoft.Json;
using PurchaseGateway.Api.Models;
using System.Net.Http.Headers;

namespace PurchaseGateway.Api.Services;

public class PaymentServiceBase : IPaymentService
{
    protected readonly HttpClient _httpClient = new HttpClient();

    private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings()
    {
        DateFormatString = "yyyy-MM-ddTHH:mm:ssZ",
    }; 

    public async Task<HealthCheckResponse> HealthCheckAsync()
    {
        var response = await _httpClient.GetAsync("payments/service-health");

        var hc = JsonConvert.DeserializeObject<HealthCheckResponse>(await response.Content.ReadAsStringAsync());

        return hc ?? new HealthCheckResponse();
    }

    public async Task<bool> TryProcessAsync(Purchase request)
    {
        var content = new StringContent(JsonConvert.SerializeObject(request, JsonSerializerSettings),
            new MediaTypeHeaderValue("application/json"));

        var response = await _httpClient.PostAsync("payments", content);

        return response.IsSuccessStatusCode;
    }

    public Task PurgeDatabaseAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "admin/purge-payments");
        request.Headers.Add("X-Rinha-Token", "123");

        return _httpClient.SendAsync(request);
    }
}
