namespace PurchaseGateway.Api.Services;

public class DefaultPaymentService : PaymentServiceBase, IPaymentService
{
    public DefaultPaymentService()
    {
        var defaultUri = Environment.GetEnvironmentVariable("PAYMENT_PROCESSOR_URL_DEFAULT")
            ?? throw new ArgumentNullException("PAYMENT_PROCESSOR_URL_DEFAULT");

        _httpClient.BaseAddress = new Uri(defaultUri);
    }
}
