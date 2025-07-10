namespace PurchaseGateway.Api.Models;

public class PaymentsSummaryResponse
{
    public required PaymentsSummary Default { get; set; }
    public required PaymentsSummary Fallback { get; set; }
}

public class PaymentsSummary
{
    public int TotalRequests { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalFee { get; set; }
    public decimal FeePerTransaction { get; set; }
}