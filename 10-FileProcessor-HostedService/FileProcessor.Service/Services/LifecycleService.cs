using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FileProcessor.Service.Services;

/// <summary>
/// Example of a service that demonstrates the full lifecycle of IHostedService
/// This shows how to handle startup, running, and shutdown phases
/// </summary>
public class LifecycleService : IHostedService, IDisposable
{
    private readonly ILogger<LifecycleService> _logger;
    private Timer _timer;
    private bool _disposed = false;

    public LifecycleService(ILogger<LifecycleService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Called when the application starts
    /// Use this for initialization that needs to complete before the application is considered "started"
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("LifecycleService is starting...");

        try
        {
            // Simulate initialization work (database connections, external service checks, etc.)
            _logger.LogInformation("Performing startup initialization...");
            await Task.Delay(2000, cancellationToken); // Simulate startup work
            
            // Start the timer for periodic work
            _timer = new Timer(DoPeriodicWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(20));
            
            _logger.LogInformation("LifecycleService started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start LifecycleService");
            throw; // Rethrow to prevent the application from starting
        }
    }

    /// <summary>
    /// Called when the application is stopping
    /// Use this for cleanup that needs to complete before the application shuts down
    /// </summary>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("LifecycleService is stopping...");

        try
        {
            // Stop the timer
            _timer?.Change(Timeout.Infinite, 0);

            // Perform cleanup work
            _logger.LogInformation("Performing shutdown cleanup...");
            await Task.Delay(1000, cancellationToken); // Simulate cleanup work

            _logger.LogInformation("LifecycleService stopped successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during LifecycleService shutdown");
            // Don't rethrow during shutdown - log and continue
        }
    }

    private void DoPeriodicWork(object state)
    {
        if (_disposed)
            return;

        try
        {
            _logger.LogInformation("LifecycleService performing periodic work at {Time}", DateTimeOffset.Now);
            
            // Example periodic work: health checks, cache refresh, metrics collection, etc.
            // This work should be lightweight and not block for long periods
            
            _logger.LogDebug("Periodic work completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during periodic work");
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _logger.LogDebug("Disposing LifecycleService");
            _timer?.Dispose();
            _disposed = true;
        }
    }
}

/// <summary>
/// Example of a service that uses IHostApplicationLifetime to respond to application lifecycle events
/// This is useful for services that need to know about application startup/shutdown events
/// </summary>
public class ApplicationLifetimeService : IHostedService
{
    private readonly ILogger<ApplicationLifetimeService> _logger;
    private readonly IHostApplicationLifetime _appLifetime;

    public ApplicationLifetimeService(
        ILogger<ApplicationLifetimeService> logger,
        IHostApplicationLifetime appLifetime)
    {
        _logger = logger;
        _appLifetime = appLifetime;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ApplicationLifetimeService starting...");

        // Register callbacks for application lifecycle events
        _appLifetime.ApplicationStarted.Register(OnStarted);
        _appLifetime.ApplicationStopping.Register(OnStopping);
        _appLifetime.ApplicationStopped.Register(OnStopped);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("ApplicationLifetimeService stopping...");
        return Task.CompletedTask;
    }

    private void OnStarted()
    {
        _logger.LogInformation("Application has fully started");
        
        // Perform work that should happen after the application is fully started
        // Examples: register with service discovery, start accepting external requests, etc.
    }

    private void OnStopping()
    {
        _logger.LogInformation("Application is stopping");
        
        // Perform work that should happen when shutdown is initiated
        // Examples: deregister from service discovery, stop accepting new requests, etc.
    }

    private void OnStopped()
    {
        _logger.LogInformation("Application has fully stopped");
        
        // Perform final cleanup work
        // Examples: flush logs, close connections, etc.
    }
}