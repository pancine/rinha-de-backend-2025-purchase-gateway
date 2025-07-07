namespace PurchaseGateway.Models;

public class PaymentsRequest
{
    public string CorrelationId { get; set; }
    public decimal Amount { get; set; }
}
