using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthDashboard.Api.HealthChecks;

/// <summary>
/// Custom health check for Redis cache connectivity
/// Demonstrates how to check cache service availability
/// This is a simulated implementation for educational purposes
/// </summary>
public class RedisHealthCheck : IHealthCheck
{
    private readonly ILogger<RedisHealthCheck> _logger;
    private readonly string _connectionString;
    private readonly TimeSpan _timeout;
    private static readonly Random _random = new();

    public RedisHealthCheck(ILogger<RedisHealthCheck> logger, string connectionString, TimeSpan timeout)
    {
        _logger = logger;
        _connectionString = connectionString;
        _timeout = timeout;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Checking Redis connectivity: {ConnectionString}", _connectionString);

            // Simulate Redis connection check
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(_timeout);

            // Simulate connection time
            var connectionTime = _random.Next(5, 100);
            await Task.Delay(connectionTime, cts.Token);

            // Simulate different scenarios
            var scenario = _random.Next(1, 101);
            var data = new Dictionary<string, object>
            {
                ["connection_string"] = _connectionString.Replace("password=", "password=***"), // Mask sensitive data
                ["connection_time_ms"] = connectionTime,
                ["timeout_ms"] = _timeout.TotalMilliseconds,
                ["check_timestamp"] = DateTime.UtcNow.ToString("O"),
                ["simulated_scenario"] = scenario
            };

            if (scenario <= 5) // 5% chance of connection failure
            {
                _logger.LogError("Redis connection failed: {ConnectionString}", _connectionString);
                return HealthCheckResult.Unhealthy(
                    "Unable to connect to Redis cache server", 
                    new Exception("Connection timeout or refused"), 
                    data);
            }
            else if (scenario <= 15) // 10% chance of high latency
            {
                var highLatency = _random.Next(200, 500);
                data["connection_time_ms"] = highLatency;
                
                _logger.LogWarning("Redis connection has high latency: {Latency}ms", highLatency);
                return HealthCheckResult.Degraded(
                    $"Redis connection is slow: {highLatency}ms", 
                    data: data);
            }
            else
            {
                // Simulate successful operations
                data["operations_tested"] = new[] { "PING", "SET", "GET", "DEL" };
                data["cache_hit_ratio"] = Math.Round(_random.NextDouble() * 100, 2);
                data["memory_usage_mb"] = _random.Next(50, 200);
                
                _logger.LogInformation("Redis is healthy with {ConnectionTime}ms response time", connectionTime);
                return HealthCheckResult.Healthy(
                    $"Redis cache is responding normally. Connection time: {connectionTime}ms", 
                    data);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("Redis health check timed out after {Timeout}ms", _timeout.TotalMilliseconds);
            return HealthCheckResult.Unhealthy(
                $"Redis health check timed out after {_timeout.TotalMilliseconds}ms");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis health check failed unexpectedly");
            return HealthCheckResult.Unhealthy(
                $"Redis health check failed: {ex.Message}", 
                ex);
        }
    }
}