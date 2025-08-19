using HealthDashboard.Api.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthDashboard.Api.Examples;

/// <summary>
/// Example configuration for registering custom health checks
/// This class demonstrates how to register the additional health checks created for this project
/// </summary>
public static class CustomHealthCheckRegistration
{
    /// <summary>
    /// Extension method to register all custom health checks
    /// This shows how you would integrate the custom health checks in a real application
    /// </summary>
    public static IServiceCollection AddCustomHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var healthChecksBuilder = services.AddHealthChecks();

        // Register Redis health check (if Redis is configured)
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            services.AddSingleton<RedisHealthCheck>(provider =>
                new RedisHealthCheck(
                    provider.GetRequiredService<ILogger<RedisHealthCheck>>(),
                    redisConnectionString,
                    TimeSpan.FromSeconds(5)));

            healthChecksBuilder.AddCheck<RedisHealthCheck>("redis-cache", 
                tags: new[] { "cache", "external", "performance" });
        }

        // Register SQL Connection health check (if SQL Server is configured)
        var sqlConnectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(sqlConnectionString))
        {
            services.AddSingleton<SqlConnectionHealthCheck>(provider =>
                new SqlConnectionHealthCheck(
                    provider.GetRequiredService<ILogger<SqlConnectionHealthCheck>>(),
                    sqlConnectionString,
                    "SELECT 1", // Simple test query
                    TimeSpan.FromSeconds(30)));

            healthChecksBuilder.AddCheck<SqlConnectionHealthCheck>("sql-database", 
                tags: new[] { "database", "critical", "ready" });
        }

        // Register Message Queue health check (if message queue is configured)
        var messageQueueConnectionString = configuration.GetConnectionString("MessageQueue");
        if (!string.IsNullOrEmpty(messageQueueConnectionString))
        {
            services.AddSingleton<MessageQueueHealthCheck>(provider =>
                new MessageQueueHealthCheck(
                    provider.GetRequiredService<ILogger<MessageQueueHealthCheck>>(),
                    "orders-queue",
                    messageQueueConnectionString,
                    maxQueueDepth: 1000));

            healthChecksBuilder.AddCheck<MessageQueueHealthCheck>("message-queue", 
                tags: new[] { "messaging", "business", "external" });
        }

        return services;
    }

    /// <summary>
    /// Example of registering health checks with different configurations
    /// This demonstrates various patterns for health check registration
    /// </summary>
    public static IServiceCollection AddAdvancedHealthCheckExamples(this IServiceCollection services)
    {
        services.AddHealthChecks()
            
            // Health check with custom timeout
            .AddCheck<ExternalApiHealthCheck>("external-api-with-timeout", 
                tags: new[] { "external", "api" })
            
            // Health check with specific failure threshold
            .AddCheck("memory-with-threshold", () =>
            {
                var memoryUsed = GC.GetTotalMemory(false);
                var threshold = 100 * 1024 * 1024; // 100MB
                
                return memoryUsed > threshold 
                    ? HealthCheckResult.Degraded($"Memory usage: {memoryUsed / (1024 * 1024)}MB")
                    : HealthCheckResult.Healthy($"Memory usage: {memoryUsed / (1024 * 1024)}MB");
            }, tags: new[] { "memory", "performance" })
            
            // Health check that runs only in specific environments
            .AddCheck("development-only", () => 
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
                    ? HealthCheckResult.Healthy("Development environment")
                    : HealthCheckResult.Healthy("Non-development environment"),
                tags: new[] { "environment" })
            
            // Composite health check that depends on multiple services
            .AddCheck("composite-business-services", () =>
            {
                // This would typically check multiple business services
                var services = new[] { "payment", "notification", "user" };
                var healthyServices = services.Where(s => CheckServiceHealth(s)).Count();
                
                if (healthyServices == services.Length)
                    return HealthCheckResult.Healthy($"All {services.Length} business services are healthy");
                else if (healthyServices > services.Length / 2)
                    return HealthCheckResult.Degraded($"{healthyServices}/{services.Length} business services are healthy");
                else
                    return HealthCheckResult.Unhealthy($"Only {healthyServices}/{services.Length} business services are healthy");
            }, tags: new[] { "business", "composite" });

        return services;
    }

    /// <summary>
    /// Example method to simulate checking a service's health
    /// In a real application, this would make actual service calls
    /// </summary>
    private static bool CheckServiceHealth(string serviceName)
    {
        // Simulate random service health for demonstration
        var random = new Random();
        return random.Next(1, 101) > 20; // 80% chance of being healthy
    }
}

/// <summary>
/// Example appsettings.json configuration for health checks
/// </summary>
public static class HealthCheckConfigurationExample
{
    public const string ExampleConfiguration = @"
{
  ""ConnectionStrings"": {
    ""DefaultConnection"": ""Server=localhost;Database=MyApp;Trusted_Connection=true;"",
    ""Redis"": ""localhost:6379"",
    ""MessageQueue"": ""amqp://guest:guest@localhost:5672/""
  },
  ""HealthChecks"": {
    ""Redis"": {
      ""Timeout"": ""00:00:05"",
      ""Enabled"": true
    },
    ""SqlServer"": {
      ""TestQuery"": ""SELECT COUNT(*) FROM Users"",
      ""Timeout"": ""00:00:30"",
      ""Enabled"": true
    },
    ""MessageQueue"": {
      ""MaxQueueDepth"": 1000,
      ""QueueName"": ""orders-queue"",
      ""Enabled"": true
    },
    ""ExternalApi"": {
      ""Url"": ""https://api.example.com/health"",
      ""Timeout"": ""00:00:10"",
      ""Enabled"": true
    }
  }
}";
}