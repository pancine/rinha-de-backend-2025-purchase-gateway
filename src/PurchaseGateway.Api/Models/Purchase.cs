namespace PurchaseGateway.Api.Models;

public class Purchase
{
    public DateTime RequestedAt { get; set; }
    public decimal Amount { get; set; }
    public int PaymentGatewayUsed { get; set; }
}
