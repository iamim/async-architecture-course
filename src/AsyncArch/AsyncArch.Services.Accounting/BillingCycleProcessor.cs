namespace AsyncArch.Services.Accounting;

public class BillingCycleProcessor : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (stoppingToken.IsCancellationRequested == false)
        {
            var now = DateOnly.FromDateTime(DateTime.UtcNow.Date);
            
            
            
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}