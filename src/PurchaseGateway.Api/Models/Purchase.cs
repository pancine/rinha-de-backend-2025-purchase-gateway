using System.ComponentModel.DataAnnotations.Schema;

namespace PurchaseGateway.Api.Models;

[Table("purchase")]
public class Purchase
{
    [Column("id")] public required string CorrelationId { get; set; }
    [Column("amount")] public decimal Amount { get; set; }
    [Column("requested_at")] public DateTime RequestedAt { get; set; }
    [Column("payment_gateway_used")] public int PaymentGatewayUsed { get; set; }
}
