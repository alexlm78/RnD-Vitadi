using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FileProcessor.Service.Configuration;
using FileProcessor.Service.Services;

namespace FileProcessor.Service;

class Program
{
    static async Task Main(string[] args)
    {
        // Create the host builder with configuration and dependency injection
        var hostBuilder = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, config) =>
            {
                // Add configuration sources
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", 
                    optional: true, reloadOnChange: true);
                config.AddEnvironmentVariables();
                config.AddCommandLine(args);
            })
            .ConfigureServices((context, services) =>
            {
                // Configure options pattern for strongly-typed configuration
                services.Configure<FileProcessorOptions>(
                    context.Configuration.GetSection(FileProcessorOptions.SectionName));
                
                // Register background task queue for queued service example
                services.AddSingleton<IBackgroundTaskQueue>(provider =>
                {
                    var logger = provider.GetRequiredService<ILogger<BackgroundTaskQueue>>();
                    return new BackgroundTaskQueue(100, logger); // Queue capacity of 100
                });
                
                // Register hosted services - you can enable/disable different examples
                
                // Main file processor service (always enabled)
                services.AddHostedService<FileProcessorService>();
                
                // Example services (uncomment to enable)
                // services.AddHostedService<TimerBasedService>();
                // services.AddHostedService<QueuedBackgroundService>();
                // services.AddHostedService<WorkItemProducer>();
                // services.AddHostedService<LifecycleService>();
                // services.AddHostedService<ApplicationLifetimeService>();
            })
            .ConfigureLogging((context, logging) =>
            {
                // Configure logging
                logging.ClearProviders();
                logging.AddConsole();
                logging.AddDebug();
                
                // Add file logging for production
                if (context.HostingEnvironment.IsProduction())
                {
                    // In a real application, you might use Serilog here
                    logging.AddEventLog(); // Windows Event Log
                }
                
                // Set minimum log level from configuration
                var logLevel = context.Configuration.GetValue<LogLevel>("Logging:LogLevel:Default", LogLevel.Information);
                logging.SetMinimumLevel(logLevel);
            });

        // Configure for Windows Service if running on Windows
        if (OperatingSystem.IsWindows())
        {
            hostBuilder.UseWindowsService(options =>
            {
                options.ServiceName = "FileProcessorService";
            });
        }
        
        // Configure for systemd if running on Linux
        if (OperatingSystem.IsLinux())
        {
            hostBuilder.UseSystemd();
        }

        var host = hostBuilder.Build();

        try
        {
            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Starting FileProcessor Service...");
            logger.LogInformation("Environment: {Environment}", host.Services.GetRequiredService<IHostEnvironment>().EnvironmentName);
            logger.LogInformation("Operating System: {OS}", Environment.OSVersion);
            
            // Start the host
            await host.RunAsync();
        }
        catch (Exception ex)
        {
            // Log any startup errors
            var logger = host.Services.GetService<ILogger<Program>>();
            logger?.LogCritical(ex, "Application terminated unexpectedly");
            throw;
        }
        finally
        {
            var logger = host.Services.GetService<ILogger<Program>>();
            logger?.LogInformation("FileProcessor Service stopped");
        }
    }
}
