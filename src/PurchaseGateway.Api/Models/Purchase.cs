namespace PurchaseGateway.Api.Models;

public class Purchase
{
    public required string CorrelationId { get; set; }
    public decimal Amount { get; set; }
    public DateTime RequestedAt { get; set; }
    public int PaymentGatewayUsed { get; set; }
}
