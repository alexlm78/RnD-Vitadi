using System.Diagnostics;
using System.Runtime.InteropServices;
using SystemMonitor.Api.Models;
using Prometheus;

namespace SystemMonitor.Api.Services;

/// <summary>
/// Service for monitoring system metrics including CPU, memory, and disk usage
/// </summary>
public class SystemMetricsService : ISystemMetricsService
{
    private readonly ILogger<SystemMetricsService> _logger;
    private readonly PerformanceCounter? _cpuCounter;
    private DateTime _lastCpuCheck = DateTime.MinValue;
    private double _lastCpuUsage = 0;
    
    // Prometheus metrics
    private static readonly Gauge CpuUsageGauge = Metrics
        .CreateGauge("system_cpu_usage_percent", "Current CPU usage percentage");
    
    private static readonly Gauge MemoryUsageGauge = Metrics
        .CreateGauge("system_memory_usage_percent", "Current memory usage percentage");
    
    private static readonly Gauge MemoryUsedBytes = Metrics
        .CreateGauge("system_memory_used_bytes", "Used memory in bytes");
    
    private static readonly Gauge MemoryTotalBytes = Metrics
        .CreateGauge("system_memory_total_bytes", "Total memory in bytes");
    
    private static readonly Gauge DiskUsageGauge = Metrics
        .CreateGauge("system_disk_usage_percent", "Disk usage percentage", "drive");
    
    private static readonly Gauge DiskUsedBytes = Metrics
        .CreateGauge("system_disk_used_bytes", "Used disk space in bytes", "drive");
    
    private static readonly Gauge DiskTotalBytes = Metrics
        .CreateGauge("system_disk_total_bytes", "Total disk space in bytes", "drive");

    public SystemMetricsService(ILogger<SystemMetricsService> logger)
    {
        _logger = logger;
        
        // Initialize CPU counter for Windows
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            try
            {
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _cpuCounter.NextValue(); // First call returns 0, so we call it once to initialize
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to initialize CPU performance counter. CPU monitoring may be limited.");
            }
        }
    }

    /// <inheritdoc />
    public async Task<SystemMetrics> GetSystemMetricsAsync()
    {
        _logger.LogDebug("Collecting system metrics");
        
        var metrics = new SystemMetrics
        {
            CpuUsagePercent = await GetCpuUsageAsync(),
            Memory = await GetMemoryUsageAsync(),
            Disks = await GetDiskUsageAsync()
        };
        
        // Update Prometheus metrics
        UpdatePrometheusMetrics(metrics);
        
        _logger.LogInformation("System metrics collected: CPU={CpuUsage:F1}%, Memory={MemoryUsage:F1}%, Disks={DiskCount}", 
            metrics.CpuUsagePercent, metrics.Memory.UsagePercent, metrics.Disks.Count);
        
        return metrics;
    }

    /// <inheritdoc />
    public async Task<double> GetCpuUsageAsync()
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && _cpuCounter != null)
            {
                // Use performance counter on Windows
                var usage = _cpuCounter.NextValue();
                _lastCpuUsage = usage;
                _lastCpuCheck = DateTime.UtcNow;
                return usage;
            }
            else
            {
                // For non-Windows platforms, use process-based estimation
                return await GetCpuUsageLinuxAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get CPU usage");
            return 0;
        }
    }

    /// <inheritdoc />
    public async Task<MemoryMetrics> GetMemoryUsageAsync()
    {
        try
        {
            var process = Process.GetCurrentProcess();
            var metrics = new MemoryMetrics
            {
                ProcessWorkingSet = process.WorkingSet64,
                ProcessPrivateMemory = process.PrivateMemorySize64
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows-specific memory information
                var memInfo = GetWindowsMemoryInfo();
                metrics.TotalPhysicalMemory = memInfo.TotalPhysicalMemory;
                metrics.AvailablePhysicalMemory = memInfo.AvailablePhysicalMemory;
            }
            else
            {
                // Linux/macOS memory information
                var memInfo = await GetLinuxMemoryInfoAsync();
                metrics.TotalPhysicalMemory = memInfo.TotalPhysicalMemory;
                metrics.AvailablePhysicalMemory = memInfo.AvailablePhysicalMemory;
            }

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get memory usage");
            return new MemoryMetrics();
        }
    }

    /// <inheritdoc />
    public async Task<List<DiskMetrics>> GetDiskUsageAsync()
    {
        var diskMetrics = new List<DiskMetrics>();
        
        try
        {
            // Use Task.Run to make the synchronous operation async
            var drives = await Task.Run(() => DriveInfo.GetDrives());
            
            foreach (var drive in drives)
            {
                try
                {
                    if (!drive.IsReady)
                    {
                        diskMetrics.Add(new DiskMetrics
                        {
                            DriveName = drive.Name,
                            DriveLabel = "Not Ready",
                            FileSystem = "Unknown",
                            IsReady = false
                        });
                        continue;
                    }

                    var metrics = new DiskMetrics
                    {
                        DriveName = drive.Name,
                        DriveLabel = string.IsNullOrEmpty(drive.VolumeLabel) ? drive.Name : drive.VolumeLabel,
                        FileSystem = drive.DriveFormat,
                        TotalSpace = drive.TotalSize,
                        FreeSpace = drive.TotalFreeSpace,
                        IsReady = true
                    };

                    diskMetrics.Add(metrics);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get metrics for drive {DriveName}", drive.Name);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get disk usage information");
        }

        return diskMetrics;
    }

    private async Task<double> GetCpuUsageLinuxAsync()
    {
        try
        {
            // Simple CPU usage estimation based on process CPU time
            var process = Process.GetCurrentProcess();
            var startTime = DateTime.UtcNow;
            var startCpuUsage = process.TotalProcessorTime;
            
            await Task.Delay(100); // Small delay to measure CPU usage
            
            var endTime = DateTime.UtcNow;
            var endCpuUsage = process.TotalProcessorTime;
            
            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
            
            return Math.Min(cpuUsageTotal * 100, 100);
        }
        catch
        {
            return 0;
        }
    }

    private (long TotalPhysicalMemory, long AvailablePhysicalMemory) GetWindowsMemoryInfo()
    {
        try
        {
            // For Windows, we'll use a simplified approach
            // In a real implementation, you might use P/Invoke to call GlobalMemoryStatusEx
            var totalMemory = GC.GetTotalMemory(false);
            var availableMemory = totalMemory / 2; // Simplified estimation
            
            return (totalMemory * 4, availableMemory); // Rough estimation
        }
        catch
        {
            return (0, 0);
        }
    }

    private async Task<(long TotalPhysicalMemory, long AvailablePhysicalMemory)> GetLinuxMemoryInfoAsync()
    {
        try
        {
            if (File.Exists("/proc/meminfo"))
            {
                var lines = await File.ReadAllLinesAsync("/proc/meminfo");
                long totalMemory = 0;
                long availableMemory = 0;

                foreach (var line in lines)
                {
                    if (line.StartsWith("MemTotal:"))
                    {
                        var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2 && long.TryParse(parts[1], out var total))
                        {
                            totalMemory = total * 1024; // Convert from KB to bytes
                        }
                    }
                    else if (line.StartsWith("MemAvailable:"))
                    {
                        var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 2 && long.TryParse(parts[1], out var available))
                        {
                            availableMemory = available * 1024; // Convert from KB to bytes
                        }
                    }
                }

                return (totalMemory, availableMemory);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read /proc/meminfo");
        }

        return (0, 0);
    }

    private void UpdatePrometheusMetrics(SystemMetrics metrics)
    {
        try
        {
            // Update CPU metrics
            CpuUsageGauge.Set(metrics.CpuUsagePercent);
            
            // Update memory metrics
            MemoryUsageGauge.Set(metrics.Memory.UsagePercent);
            MemoryUsedBytes.Set(metrics.Memory.UsedPhysicalMemory);
            MemoryTotalBytes.Set(metrics.Memory.TotalPhysicalMemory);
            
            // Update disk metrics
            foreach (var disk in metrics.Disks.Where(d => d.IsReady))
            {
                var driveName = disk.DriveName.Replace(":", "").Replace("\\", "").Replace("/", "root");
                DiskUsageGauge.WithLabels(driveName).Set(disk.UsagePercent);
                DiskUsedBytes.WithLabels(driveName).Set(disk.UsedSpace);
                DiskTotalBytes.WithLabels(driveName).Set(disk.TotalSpace);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to update Prometheus metrics");
        }
    }

    public void Dispose()
    {
        _cpuCounter?.Dispose();
    }
}