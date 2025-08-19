using SystemMonitor.Api.Services;

namespace SystemMonitor.Api.Services;

/// <summary>
/// Background service that periodically collects system metrics
/// </summary>
public class MetricsCollectionService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MetricsCollectionService> _logger;
    private readonly IConfiguration _configuration;
    private readonly TimeSpan _collectionInterval;

    public MetricsCollectionService(
        IServiceProvider serviceProvider, 
        ILogger<MetricsCollectionService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
        
        // Get collection interval from configuration, default to 30 seconds
        var intervalSeconds = _configuration.GetValue<int>("MetricsCollection:IntervalSeconds", 30);
        _collectionInterval = TimeSpan.FromSeconds(intervalSeconds);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Metrics collection service started with interval {Interval}", _collectionInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var metricsService = scope.ServiceProvider.GetRequiredService<ISystemMetricsService>();
                
                // Collect metrics - this will automatically update Prometheus gauges
                var metrics = await metricsService.GetSystemMetricsAsync();
                
                _logger.LogDebug("Metrics collected: CPU={CpuUsage:F1}%, Memory={MemoryUsage:F1}%, Disks={DiskCount}",
                    metrics.CpuUsagePercent, metrics.Memory.UsagePercent, metrics.Disks.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while collecting metrics");
            }

            try
            {
                await Task.Delay(_collectionInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
        }

        _logger.LogInformation("Metrics collection service stopped");
    }
}