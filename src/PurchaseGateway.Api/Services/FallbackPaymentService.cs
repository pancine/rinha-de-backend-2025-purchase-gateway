namespace PurchaseGateway.Api.Services;

public class FallbackPaymentService : PaymentServiceBase, IPaymentService
{
    public FallbackPaymentService()
    {
        var defaultUri = Environment.GetEnvironmentVariable("PAYMENT_PROCESSOR_URL_FALLBACK")
            ?? throw new ArgumentNullException("PAYMENT_PROCESSOR_URL_FALLBACK");

        _httpClient.BaseAddress = new Uri(defaultUri);
    }
}