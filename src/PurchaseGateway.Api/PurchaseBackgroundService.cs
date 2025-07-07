namespace PurchaseGateway;

public class PurchaseBackgroundService(ILogger<PurchaseBackgroundService> logger) : BackgroundService
{
    private readonly ILogger<PurchaseBackgroundService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Get Service 1 status");
            _logger.LogInformation("Get Service 2 status");

            await Task.Delay(5000, stoppingToken);
        }
    }
}
