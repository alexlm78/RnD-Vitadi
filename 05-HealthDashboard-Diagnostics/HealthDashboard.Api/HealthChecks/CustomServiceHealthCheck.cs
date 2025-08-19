using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthDashboard.Api.HealthChecks;

/// <summary>
/// Custom health check for a simulated service dependency
/// Demonstrates how to create health checks for custom business logic
/// </summary>
public class CustomServiceHealthCheck : IHealthCheck
{
    private readonly ILogger<CustomServiceHealthCheck> _logger;
    private readonly string _serviceName;
    private static readonly Random _random = new();

    public CustomServiceHealthCheck(ILogger<CustomServiceHealthCheck> logger, string serviceName)
    {
        _logger = logger;
        _serviceName = serviceName;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Checking custom service health: {ServiceName}", _serviceName);

            // Simulate some business logic check
            var simulatedLatency = _random.Next(10, 500); // Random latency between 10-500ms
            var simulatedSuccess = _random.Next(1, 101); // Random number 1-100 for success rate

            var data = new Dictionary<string, object>
            {
                ["service_name"] = _serviceName,
                ["simulated_latency_ms"] = simulatedLatency,
                ["check_timestamp"] = DateTime.UtcNow.ToString("O"),
                ["success_probability"] = simulatedSuccess
            };

            // Simulate different health states based on random values
            if (simulatedSuccess <= 10) // 10% chance of unhealthy
            {
                _logger.LogError("Custom service is unhealthy: {ServiceName}", _serviceName);
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    $"Service '{_serviceName}' is experiencing critical issues", 
                    data: data));
            }
            else if (simulatedSuccess <= 25) // 15% chance of degraded (25% - 10%)
            {
                _logger.LogWarning("Custom service is degraded: {ServiceName}", _serviceName);
                return Task.FromResult(HealthCheckResult.Degraded(
                    $"Service '{_serviceName}' is experiencing performance issues. Latency: {simulatedLatency}ms", 
                    data: data));
            }
            else if (simulatedLatency > 300) // High latency = degraded
            {
                _logger.LogWarning("Custom service has high latency: {ServiceName}, {Latency}ms", _serviceName, simulatedLatency);
                return Task.FromResult(HealthCheckResult.Degraded(
                    $"Service '{_serviceName}' has high latency: {simulatedLatency}ms", 
                    data: data));
            }
            else
            {
                _logger.LogInformation("Custom service is healthy: {ServiceName}, {Latency}ms", _serviceName, simulatedLatency);
                return Task.FromResult(HealthCheckResult.Healthy(
                    $"Service '{_serviceName}' is operating normally. Latency: {simulatedLatency}ms", 
                    data: data));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Custom service health check failed: {ServiceName}", _serviceName);
            return Task.FromResult(HealthCheckResult.Unhealthy($"Service '{_serviceName}' health check failed: {ex.Message}", ex));
        }
    }
}