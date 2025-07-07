
namespace PurchaseGateway.Services;

public class DefaultPaymentService : PaymentServiceBase, IPaymentService
{
    public DefaultPaymentService(HttpClient httpClient) : base(httpClient)
    {
        var defaultUri = Environment.GetEnvironmentVariable("PAYMENT_PROCESSOR_URL_DEFAULT")
            ?? throw new ArgumentNullException("PAYMENT_PROCESSOR_URL_DEFAULT");

        httpClient.BaseAddress = new Uri(defaultUri);
    }
}

public class FallbackPaymentServices : PaymentServiceBase, IPaymentService
{
    public FallbackPaymentServices(HttpClient httpClient) : base(httpClient)
    {
        httpClient.BaseAddress = new Uri(Environment.GetEnvironmentVariable("PAYMENT_PROCESSOR_URL_FALLBACK"));
    }
}