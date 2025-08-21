using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace FileProcessor.Service.Services;

/// <summary>
/// Interface for a background task queue
/// </summary>
public interface IBackgroundTaskQueue
{
    ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask> workItem);
    ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Implementation of a background task queue using channels
/// </summary>
public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly System.Threading.Channels.Channel<Func<CancellationToken, ValueTask>> _queue;
    private readonly ILogger<BackgroundTaskQueue> _logger;

    public BackgroundTaskQueue(int capacity, ILogger<BackgroundTaskQueue> logger)
    {
        _logger = logger;
        
        // Create a bounded channel with the specified capacity
        var options = new System.Threading.Channels.BoundedChannelOptions(capacity)
        {
            FullMode = System.Threading.Channels.BoundedChannelFullMode.Wait
        };
        
        _queue = System.Threading.Channels.Channel.CreateBounded<Func<CancellationToken, ValueTask>>(options);
    }

    public async ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask> workItem)
    {
        if (workItem == null)
        {
            throw new ArgumentNullException(nameof(workItem));
        }

        await _queue.Writer.WriteAsync(workItem);
        _logger.LogDebug("Work item queued");
    }

    public async ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(CancellationToken cancellationToken)
    {
        var workItem = await _queue.Reader.ReadAsync(cancellationToken);
        _logger.LogDebug("Work item dequeued");
        return workItem;
    }
}

/// <summary>
/// Example of a queued background service that processes work items from a queue
/// This is useful for handling work items that are queued from other parts of the application
/// </summary>
public class QueuedBackgroundService : BackgroundService
{
    private readonly ILogger<QueuedBackgroundService> _logger;
    private readonly IBackgroundTaskQueue _taskQueue;

    public QueuedBackgroundService(
        IBackgroundTaskQueue taskQueue,
        ILogger<QueuedBackgroundService> logger)
    {
        _taskQueue = taskQueue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("QueuedBackgroundService starting...");

        await BackgroundProcessing(stoppingToken);
    }

    private async Task BackgroundProcessing(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var workItem = await _taskQueue.DequeueAsync(stoppingToken);

                try
                {
                    _logger.LogDebug("Processing queued work item");
                    await workItem(stoppingToken);
                    _logger.LogDebug("Queued work item processed successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing work item");
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in QueuedBackgroundService");
                // Add a delay to prevent tight loop in case of persistent errors
                await Task.Delay(1000, stoppingToken);
            }
        }

        _logger.LogInformation("QueuedBackgroundService stopped");
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("QueuedBackgroundService is stopping...");
        await base.StopAsync(stoppingToken);
    }
}

/// <summary>
/// Example service that demonstrates how to queue work items
/// In a real application, this could be a controller, another service, etc.
/// </summary>
public class WorkItemProducer : IHostedService
{
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly ILogger<WorkItemProducer> _logger;
    private readonly Timer _timer;

    public WorkItemProducer(
        IBackgroundTaskQueue taskQueue,
        ILogger<WorkItemProducer> logger)
    {
        _taskQueue = taskQueue;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("WorkItemProducer starting...");

        // Queue some example work items every 15 seconds
        _timer = new Timer(QueueWorkItems, null, TimeSpan.Zero, TimeSpan.FromSeconds(15));

        return Task.CompletedTask;
    }

    private async void QueueWorkItems(object state)
    {
        try
        {
            // Queue different types of work items
            await _taskQueue.QueueBackgroundWorkItemAsync(async token =>
            {
                _logger.LogInformation("Processing email notification work item");
                await Task.Delay(2000, token); // Simulate email sending
                _logger.LogInformation("Email notification sent");
            });

            await _taskQueue.QueueBackgroundWorkItemAsync(async token =>
            {
                _logger.LogInformation("Processing data cleanup work item");
                await Task.Delay(1500, token); // Simulate data cleanup
                _logger.LogInformation("Data cleanup completed");
            });

            await _taskQueue.QueueBackgroundWorkItemAsync(async token =>
            {
                _logger.LogInformation("Processing report generation work item");
                await Task.Delay(3000, token); // Simulate report generation
                _logger.LogInformation("Report generated");
            });

            _logger.LogDebug("Work items queued successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error queueing work items");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("WorkItemProducer stopping...");
        _timer?.Change(Timeout.Infinite, 0);
        _timer?.Dispose();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}