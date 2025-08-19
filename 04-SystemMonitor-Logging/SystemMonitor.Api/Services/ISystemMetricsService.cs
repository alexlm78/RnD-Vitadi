using SystemMonitor.Api.Models;

namespace SystemMonitor.Api.Services;

/// <summary>
/// Interface for system metrics monitoring service
/// </summary>
public interface ISystemMetricsService
{
    /// <summary>
    /// Gets current system metrics including CPU, memory, and disk usage
    /// </summary>
    /// <returns>Current system metrics</returns>
    Task<SystemMetrics> GetSystemMetricsAsync();
    
    /// <summary>
    /// Gets CPU usage percentage
    /// </summary>
    /// <returns>CPU usage as percentage (0-100)</returns>
    Task<double> GetCpuUsageAsync();
    
    /// <summary>
    /// Gets memory usage information
    /// </summary>
    /// <returns>Memory usage details</returns>
    Task<MemoryMetrics> GetMemoryUsageAsync();
    
    /// <summary>
    /// Gets disk usage information for all drives
    /// </summary>
    /// <returns>List of disk usage information</returns>
    Task<List<DiskMetrics>> GetDiskUsageAsync();
}