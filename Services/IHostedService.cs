public class DailySummaryHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private Timer _timer;

    public DailySummaryHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(GenerateDailySummary, null, TimeSpan.Zero, TimeSpan.FromDays(1));
        return Task.CompletedTask;
    }

    private async void GenerateDailySummary(object state)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var summaryService = scope.ServiceProvider.GetRequiredService<DailySummaryService>();
            await summaryService.GenerateDailySummary(DateTime.UtcNow);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }
}
