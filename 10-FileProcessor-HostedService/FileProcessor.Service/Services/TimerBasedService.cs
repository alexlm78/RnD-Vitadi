using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FileProcessor.Service.Services;

/// <summary>
/// Example of a timer-based background service that executes work at regular intervals
/// This is useful for periodic tasks like cleanup, health checks, or data synchronization
/// </summary>
public class TimerBasedService : BackgroundService
{
    private readonly ILogger<TimerBasedService> _logger;
    private readonly Timer _timer;
    private int _executionCount = 0;

    public TimerBasedService(ILogger<TimerBasedService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TimerBasedService starting...");

        // Create a timer that executes every 10 seconds
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await DoWorkAsync();
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
            _logger.LogInformation("TimerBasedService is stopping due to cancellation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in TimerBasedService");
            throw;
        }
        finally
        {
            _logger.LogInformation("TimerBasedService stopped");
        }
    }

    private async Task DoWorkAsync()
    {
        _executionCount++;
        _logger.LogInformation("TimerBasedService executing work iteration {Count} at {Time}", 
            _executionCount, DateTimeOffset.Now);

        try
        {
            // Simulate some work
            await Task.Delay(1000);
            
            // Example: Cleanup old files, check system health, sync data, etc.
            _logger.LogDebug("Work completed successfully for iteration {Count}", _executionCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during work execution in iteration {Count}", _executionCount);
            // Don't rethrow - let the service continue running
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("TimerBasedService is stopping...");
        await base.StopAsync(stoppingToken);
    }
}