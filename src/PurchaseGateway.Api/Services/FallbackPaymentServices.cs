namespace PurchaseGateway.Api.Services;

public class FallbackPaymentServices : PaymentServiceBase, IPaymentService
{
    public FallbackPaymentServices()
    {
        var defaultUri = Environment.GetEnvironmentVariable("PAYMENT_PROCESSOR_URL_FALLBACK")
            ?? throw new ArgumentNullException("PAYMENT_PROCESSOR_URL_FALLBACK");

        _httpClient.BaseAddress = new Uri(defaultUri);
    }
}