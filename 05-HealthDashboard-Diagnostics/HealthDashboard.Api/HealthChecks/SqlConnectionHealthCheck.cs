using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Data;

namespace HealthDashboard.Api.HealthChecks;

/// <summary>
/// Custom health check for SQL Server database connectivity
/// Demonstrates how to check database connections with custom queries
/// This is a simulated implementation for educational purposes
/// </summary>
public class SqlConnectionHealthCheck : IHealthCheck
{
    private readonly ILogger<SqlConnectionHealthCheck> _logger;
    private readonly string _connectionString;
    private readonly string _testQuery;
    private readonly TimeSpan _timeout;
    private static readonly Random _random = new();

    public SqlConnectionHealthCheck(
        ILogger<SqlConnectionHealthCheck> logger, 
        string connectionString, 
        string testQuery = "SELECT 1", 
        TimeSpan? timeout = null)
    {
        _logger = logger;
        _connectionString = connectionString;
        _testQuery = testQuery;
        _timeout = timeout ?? TimeSpan.FromSeconds(30);
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Checking SQL Server connectivity with query: {Query}", _testQuery);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(_timeout);

            // Simulate database connection and query execution
            var startTime = DateTime.UtcNow;
            
            // Simulate connection time
            var connectionTime = _random.Next(10, 200);
            await Task.Delay(connectionTime, cts.Token);
            
            // Simulate query execution time
            var queryTime = _random.Next(5, 100);
            await Task.Delay(queryTime, cts.Token);
            
            var totalTime = DateTime.UtcNow - startTime;
            
            // Simulate different database scenarios
            var scenario = _random.Next(1, 101);
            var data = new Dictionary<string, object>
            {
                ["connection_string"] = MaskConnectionString(_connectionString),
                ["test_query"] = _testQuery,
                ["connection_time_ms"] = connectionTime,
                ["query_execution_time_ms"] = queryTime,
                ["total_time_ms"] = Math.Round(totalTime.TotalMilliseconds, 2),
                ["timeout_ms"] = _timeout.TotalMilliseconds,
                ["check_timestamp"] = DateTime.UtcNow.ToString("O")
            };

            if (scenario <= 3) // 3% chance of connection failure
            {
                _logger.LogError("SQL Server connection failed");
                return HealthCheckResult.Unhealthy(
                    "Unable to connect to SQL Server database", 
                    new Exception("Login failed or server not accessible"), 
                    data);
            }
            else if (scenario <= 8) // 5% chance of query timeout
            {
                _logger.LogError("SQL Server query timed out: {Query}", _testQuery);
                return HealthCheckResult.Unhealthy(
                    $"Database query timed out: {_testQuery}", 
                    new TimeoutException("Query execution exceeded timeout"), 
                    data);
            }
            else if (scenario <= 20) // 12% chance of performance issues
            {
                var slowQueryTime = _random.Next(300, 1000);
                data["query_execution_time_ms"] = slowQueryTime;
                data["total_time_ms"] = connectionTime + slowQueryTime;
                
                _logger.LogWarning("SQL Server query is slow: {QueryTime}ms", slowQueryTime);
                return HealthCheckResult.Degraded(
                    $"Database query is slow: {slowQueryTime}ms", 
                    data: data);
            }
            else
            {
                // Simulate successful database metrics
                data["database_size_mb"] = _random.Next(100, 5000);
                data["active_connections"] = _random.Next(5, 50);
                data["cpu_usage_percent"] = Math.Round(_random.NextDouble() * 30, 2); // 0-30% CPU
                data["memory_usage_mb"] = _random.Next(512, 2048);
                data["last_backup"] = DateTime.UtcNow.AddHours(-_random.Next(1, 24)).ToString("O");
                
                _logger.LogInformation("SQL Server is healthy. Query executed in {QueryTime}ms", queryTime);
                return HealthCheckResult.Healthy(
                    $"Database is responding normally. Query time: {queryTime}ms", 
                    data);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("SQL Server health check timed out after {Timeout}ms", _timeout.TotalMilliseconds);
            return HealthCheckResult.Unhealthy(
                $"Database health check timed out after {_timeout.TotalMilliseconds}ms");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SQL Server health check failed unexpectedly");
            return HealthCheckResult.Unhealthy(
                $"Database health check failed: {ex.Message}", 
                ex);
        }
    }

    private static string MaskConnectionString(string connectionString)
    {
        // Simple masking of sensitive information in connection strings
        return connectionString
            .Replace("Password=", "Password=***;", StringComparison.OrdinalIgnoreCase)
            .Replace("pwd=", "pwd=***;", StringComparison.OrdinalIgnoreCase)
            .Replace("User ID=", "User ID=***;", StringComparison.OrdinalIgnoreCase)
            .Replace("uid=", "uid=***;", StringComparison.OrdinalIgnoreCase);
    }
}