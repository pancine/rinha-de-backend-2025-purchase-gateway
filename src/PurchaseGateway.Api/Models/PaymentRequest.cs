namespace PurchaseGateway.Api.Models;

public class PaymentRequest
{
    public string CorrelationId { get; set; }
    public decimal Amount { get; set; }
    public string? RequestedAt { get; set; }
}
