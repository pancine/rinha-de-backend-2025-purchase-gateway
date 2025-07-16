namespace PurchaseGateway.Api.Models;

public class PaymentsSummaryResponse
{
    public required PaymentsSummary Default { get; set; }
    public required PaymentsSummary Fallback { get; set; }
}

public class PaymentsSummary
{
    public long TotalRequests { get; set; }
    public decimal TotalAmount { get; set; }
}