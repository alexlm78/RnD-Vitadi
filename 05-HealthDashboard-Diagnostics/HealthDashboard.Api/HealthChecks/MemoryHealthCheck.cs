using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthDashboard.Api.HealthChecks;

/// <summary>
/// Custom health check for memory usage
/// Demonstrates how to check system resource usage
/// </summary>
public class MemoryHealthCheck : IHealthCheck
{
    private readonly ILogger<MemoryHealthCheck> _logger;
    private readonly long _maxMemoryBytes;

    public MemoryHealthCheck(ILogger<MemoryHealthCheck> logger, long maxMemoryBytes = 1024 * 1024 * 1024) // 1GB default
    {
        _logger = logger;
        _maxMemoryBytes = maxMemoryBytes;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Checking memory usage health");

            // Get current memory usage
            var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
            var workingSet = currentProcess.WorkingSet64;
            var privateMemory = currentProcess.PrivateMemorySize64;
            
            // Get GC memory info
            var gcMemoryInfo = GC.GetGCMemoryInfo();
            var totalMemory = GC.GetTotalMemory(false);
            
            var data = new Dictionary<string, object>
            {
                ["working_set_bytes"] = workingSet,
                ["private_memory_bytes"] = privateMemory,
                ["gc_total_memory_bytes"] = totalMemory,
                ["gc_heap_size_bytes"] = gcMemoryInfo.HeapSizeBytes,
                ["gc_memory_load_bytes"] = gcMemoryInfo.MemoryLoadBytes,
                ["max_allowed_memory_bytes"] = _maxMemoryBytes,
                ["memory_usage_percentage"] = Math.Round((double)workingSet / _maxMemoryBytes * 100, 2)
            };

            if (workingSet > _maxMemoryBytes)
            {
                _logger.LogError("Memory usage is too high: {WorkingSet}MB, max allowed: {MaxMemory}MB", 
                    workingSet / (1024 * 1024), _maxMemoryBytes / (1024 * 1024));
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    $"Memory usage is too high: {workingSet / (1024 * 1024)}MB, max allowed: {_maxMemoryBytes / (1024 * 1024)}MB", 
                    data: data));
            }

            var warningThreshold = _maxMemoryBytes * 0.8; // 80% threshold
            if (workingSet > warningThreshold)
            {
                _logger.LogWarning("Memory usage is high: {WorkingSet}MB, warning threshold: {WarningThreshold}MB", 
                    workingSet / (1024 * 1024), warningThreshold / (1024 * 1024));
                return Task.FromResult(HealthCheckResult.Degraded(
                    $"Memory usage is high: {workingSet / (1024 * 1024)}MB", 
                    data: data));
            }

            _logger.LogInformation("Memory usage is healthy: {WorkingSet}MB", workingSet / (1024 * 1024));
            return Task.FromResult(HealthCheckResult.Healthy(
                $"Memory usage is healthy: {workingSet / (1024 * 1024)}MB", 
                data: data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Memory health check failed");
            return Task.FromResult(HealthCheckResult.Unhealthy($"Memory health check failed: {ex.Message}", ex));
        }
    }
}