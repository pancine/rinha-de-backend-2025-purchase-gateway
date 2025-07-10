namespace PurchaseGateway.Api.Models;

public class PaymentsSummaryResponse
{
    public required ProcessedRequests Default { get; set; }
    public required ProcessedRequests Fallback { get; set; }
}

public class ProcessedRequests
{
    public int TotalRequests { get; set; }
    public decimal TotalAmount { get; set; }
}