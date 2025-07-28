namespace PurchaseGateway.Api.Models;

public record struct PaymentsSummaryAggregationResult(int PaymentGatewayUsed, long TotalRequests, decimal TotalAmount);