namespace PurchaseGateway.Models;

public class ProcessRequest : PaymentsRequest
{
    public DateTime RequestedAt { get; set; }
}
