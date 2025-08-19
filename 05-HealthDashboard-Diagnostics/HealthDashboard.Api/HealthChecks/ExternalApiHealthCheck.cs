using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Net;

namespace HealthDashboard.Api.HealthChecks;

/// <summary>
/// Custom health check for external API services
/// Demonstrates how to check external dependencies
/// </summary>
public class ExternalApiHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ExternalApiHealthCheck> _logger;
    private readonly string _url;
    private readonly TimeSpan _timeout;

    public ExternalApiHealthCheck(HttpClient httpClient, ILogger<ExternalApiHealthCheck> logger, string url, TimeSpan timeout)
    {
        _httpClient = httpClient;
        _logger = logger;
        _url = url;
        _timeout = timeout;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Checking external API health: {Url}", _url);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(_timeout);

            var response = await _httpClient.GetAsync(_url, cts.Token);
            
            var data = new Dictionary<string, object>
            {
                ["url"] = _url,
                ["status_code"] = (int)response.StatusCode,
                ["response_time"] = DateTime.UtcNow.ToString("O")
            };

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("External API is healthy: {Url} returned {StatusCode}", _url, response.StatusCode);
                return HealthCheckResult.Healthy($"External API is responding correctly. Status: {response.StatusCode}", data);
            }
            else if (response.StatusCode == HttpStatusCode.ServiceUnavailable || 
                     response.StatusCode == HttpStatusCode.RequestTimeout)
            {
                _logger.LogWarning("External API is degraded: {Url} returned {StatusCode}", _url, response.StatusCode);
                return HealthCheckResult.Degraded($"External API returned {response.StatusCode}", data: data);
            }
            else
            {
                _logger.LogError("External API is unhealthy: {Url} returned {StatusCode}", _url, response.StatusCode);
                return HealthCheckResult.Unhealthy($"External API returned {response.StatusCode}", data: data);
            }
        }
        catch (TaskCanceledException ex) when (ex.CancellationToken.IsCancellationRequested)
        {
            _logger.LogError("External API health check timed out: {Url}", _url);
            return HealthCheckResult.Unhealthy($"External API health check timed out after {_timeout.TotalSeconds} seconds", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "External API health check failed: {Url}", _url);
            return HealthCheckResult.Unhealthy($"External API health check failed: {ex.Message}", ex);
        }
    }
}