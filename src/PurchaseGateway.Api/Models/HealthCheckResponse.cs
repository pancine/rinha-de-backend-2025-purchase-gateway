namespace PurchaseGateway.Models;

public class HealthCheckResponse
{
    public bool Failing { get; set; } = false;
    public int MinResponseTime { get; set; }
}
