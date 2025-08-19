using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthDashboard.Api.Controllers;

/// <summary>
/// Controller for health check endpoints and dashboard functionality
/// Demonstrates how to work with health checks programmatically
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly HealthCheckService _healthCheckService;
    private readonly ILogger<HealthController> _logger;

    public HealthController(HealthCheckService healthCheckService, ILogger<HealthController> logger)
    {
        _healthCheckService = healthCheckService;
        _logger = logger;
    }

    /// <summary>
    /// Get detailed health check results
    /// </summary>
    /// <returns>Detailed health check information</returns>
    [HttpGet("detailed")]
    public async Task<IActionResult> GetDetailedHealth()
    {
        try
        {
            var healthReport = await _healthCheckService.CheckHealthAsync();
            
            var response = new
            {
                Status = healthReport.Status.ToString(),
                TotalDuration = healthReport.TotalDuration.TotalMilliseconds,
                Checks = healthReport.Entries.Select(entry => new
                {
                    Name = entry.Key,
                    Status = entry.Value.Status.ToString(),
                    Duration = entry.Value.Duration.TotalMilliseconds,
                    Description = entry.Value.Description,
                    Tags = entry.Value.Tags,
                    Exception = entry.Value.Exception?.Message
                })
            };

            _logger.LogInformation("Health check completed with status: {Status}", healthReport.Status);

            return healthReport.Status == HealthStatus.Healthy 
                ? Ok(response) 
                : StatusCode(503, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking health");
            return StatusCode(500, new { Error = "Health check failed", Message = ex.Message });
        }
    }

    /// <summary>
    /// Get simple health status
    /// </summary>
    /// <returns>Simple health status</returns>
    [HttpGet("status")]
    public async Task<IActionResult> GetHealthStatus()
    {
        try
        {
            var healthReport = await _healthCheckService.CheckHealthAsync();
            
            var response = new
            {
                Status = healthReport.Status.ToString(),
                Timestamp = DateTime.UtcNow,
                TotalChecks = healthReport.Entries.Count,
                HealthyChecks = healthReport.Entries.Count(e => e.Value.Status == HealthStatus.Healthy),
                UnhealthyChecks = healthReport.Entries.Count(e => e.Value.Status == HealthStatus.Unhealthy),
                DegradedChecks = healthReport.Entries.Count(e => e.Value.Status == HealthStatus.Degraded)
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting health status");
            return StatusCode(500, new { Error = "Failed to get health status", Message = ex.Message });
        }
    }

    /// <summary>
    /// Get health checks by category/tag
    /// </summary>
    /// <param name="tag">Tag to filter health checks</param>
    /// <returns>Health checks filtered by tag</returns>
    [HttpGet("category/{tag}")]
    public async Task<IActionResult> GetHealthByCategory(string tag)
    {
        try
        {
            var healthReport = await _healthCheckService.CheckHealthAsync();
            
            var filteredChecks = healthReport.Entries
                .Where(entry => entry.Value.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
                .Select(entry => new
                {
                    Name = entry.Key,
                    Status = entry.Value.Status.ToString(),
                    Duration = entry.Value.Duration.TotalMilliseconds,
                    Description = entry.Value.Description,
                    Tags = entry.Value.Tags,
                    Data = entry.Value.Data,
                    Exception = entry.Value.Exception?.Message
                });

            var response = new
            {
                Category = tag,
                Status = filteredChecks.Any() ? 
                    filteredChecks.All(c => c.Status == "Healthy") ? "Healthy" :
                    filteredChecks.Any(c => c.Status == "Unhealthy") ? "Unhealthy" : "Degraded"
                    : "Unknown",
                Checks = filteredChecks
            };

            _logger.LogInformation("Health check category '{Category}' requested, found {Count} checks", tag, filteredChecks.Count());

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting health checks for category: {Category}", tag);
            return StatusCode(500, new { Error = "Failed to get health checks by category", Message = ex.Message });
        }
    }

    /// <summary>
    /// Get available health check categories
    /// </summary>
    /// <returns>List of available categories/tags</returns>
    [HttpGet("categories")]
    public async Task<IActionResult> GetHealthCategories()
    {
        try
        {
            var healthReport = await _healthCheckService.CheckHealthAsync();
            
            var categories = healthReport.Entries
                .SelectMany(entry => entry.Value.Tags)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(tag => tag)
                .Select(tag => new
                {
                    Name = tag,
                    CheckCount = healthReport.Entries.Count(e => e.Value.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
                });

            return Ok(new { Categories = categories });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting health check categories");
            return StatusCode(500, new { Error = "Failed to get health check categories", Message = ex.Message });
        }
    }

    /// <summary>
    /// Get HTML dashboard view
    /// </summary>
    /// <returns>HTML dashboard for health checks</returns>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetHealthDashboard()
    {
        try
        {
            var htmlPath = Path.Combine(Directory.GetCurrentDirectory(), "Views", "HealthDashboard.html");
            
            if (!System.IO.File.Exists(htmlPath))
            {
                _logger.LogError("Health dashboard HTML file not found at: {Path}", htmlPath);
                return NotFound("Health dashboard not found");
            }

            var htmlContent = await System.IO.File.ReadAllTextAsync(htmlPath);
            return Content(htmlContent, "text/html");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while serving health dashboard");
            return StatusCode(500, "Failed to load health dashboard");
        }
    }

    /// <summary>
    /// Get health check summary for quick overview
    /// </summary>
    /// <returns>Summary of all health checks</returns>
    [HttpGet("summary")]
    public async Task<IActionResult> GetHealthSummary()
    {
        try
        {
            var healthReport = await _healthCheckService.CheckHealthAsync();
            
            var summary = new
            {
                OverallStatus = healthReport.Status.ToString(),
                Timestamp = DateTime.UtcNow,
                TotalDuration = Math.Round(healthReport.TotalDuration.TotalMilliseconds, 2),
                Statistics = new
                {
                    Total = healthReport.Entries.Count,
                    Healthy = healthReport.Entries.Count(e => e.Value.Status == HealthStatus.Healthy),
                    Degraded = healthReport.Entries.Count(e => e.Value.Status == HealthStatus.Degraded),
                    Unhealthy = healthReport.Entries.Count(e => e.Value.Status == HealthStatus.Unhealthy)
                },
                Categories = healthReport.Entries
                    .SelectMany(entry => entry.Value.Tags)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(tag => tag)
                    .Select(tag => new
                    {
                        Name = tag,
                        CheckCount = healthReport.Entries.Count(e => e.Value.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase)),
                        HealthyCount = healthReport.Entries.Count(e => e.Value.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase) && e.Value.Status == HealthStatus.Healthy),
                        Status = healthReport.Entries.Where(e => e.Value.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
                            .All(e => e.Value.Status == HealthStatus.Healthy) ? "Healthy" :
                            healthReport.Entries.Where(e => e.Value.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
                            .Any(e => e.Value.Status == HealthStatus.Unhealthy) ? "Unhealthy" : "Degraded"
                    }),
                RecentChecks = healthReport.Entries
                    .OrderByDescending(e => e.Value.Duration)
                    .Take(5)
                    .Select(entry => new
                    {
                        Name = entry.Key,
                        Status = entry.Value.Status.ToString(),
                        Duration = Math.Round(entry.Value.Duration.TotalMilliseconds, 2)
                    })
            };

            _logger.LogInformation("Health summary requested - Overall status: {Status}", healthReport.Status);

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting health summary");
            return StatusCode(500, new { Error = "Failed to get health summary", Message = ex.Message });
        }
    }
}