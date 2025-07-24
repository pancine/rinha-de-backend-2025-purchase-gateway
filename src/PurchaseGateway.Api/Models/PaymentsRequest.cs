namespace PurchaseGateway.Api.Models;

public class PaymentsRequest
{
    public required string CorrelationId { get; set; }
    public required decimal Amount { get; set; }
}