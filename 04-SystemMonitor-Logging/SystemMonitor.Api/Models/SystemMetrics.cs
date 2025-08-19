namespace SystemMonitor.Api.Models;

/// <summary>
/// Represents comprehensive system metrics
/// </summary>
public class SystemMetrics
{
    /// <summary>
    /// CPU usage percentage (0-100)
    /// </summary>
    public double CpuUsagePercent { get; set; }
    
    /// <summary>
    /// Memory usage information
    /// </summary>
    public MemoryMetrics Memory { get; set; } = new();
    
    /// <summary>
    /// Disk usage information for all drives
    /// </summary>
    public List<DiskMetrics> Disks { get; set; } = new();
    
    /// <summary>
    /// Timestamp when metrics were collected
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Machine name where metrics were collected
    /// </summary>
    public string MachineName { get; set; } = Environment.MachineName;
}

/// <summary>
/// Represents memory usage metrics
/// </summary>
public class MemoryMetrics
{
    /// <summary>
    /// Total physical memory in bytes
    /// </summary>
    public long TotalPhysicalMemory { get; set; }
    
    /// <summary>
    /// Available physical memory in bytes
    /// </summary>
    public long AvailablePhysicalMemory { get; set; }
    
    /// <summary>
    /// Used physical memory in bytes
    /// </summary>
    public long UsedPhysicalMemory => TotalPhysicalMemory - AvailablePhysicalMemory;
    
    /// <summary>
    /// Memory usage percentage (0-100)
    /// </summary>
    public double UsagePercent => TotalPhysicalMemory > 0 
        ? (double)UsedPhysicalMemory / TotalPhysicalMemory * 100 
        : 0;
    
    /// <summary>
    /// Working set of current process in bytes
    /// </summary>
    public long ProcessWorkingSet { get; set; }
    
    /// <summary>
    /// Private memory size of current process in bytes
    /// </summary>
    public long ProcessPrivateMemory { get; set; }
}

/// <summary>
/// Represents disk usage metrics for a single drive
/// </summary>
public class DiskMetrics
{
    /// <summary>
    /// Drive name (e.g., "C:\", "/")
    /// </summary>
    public string DriveName { get; set; } = string.Empty;
    
    /// <summary>
    /// Drive label/name
    /// </summary>
    public string DriveLabel { get; set; } = string.Empty;
    
    /// <summary>
    /// File system type (e.g., NTFS, ext4)
    /// </summary>
    public string FileSystem { get; set; } = string.Empty;
    
    /// <summary>
    /// Total disk space in bytes
    /// </summary>
    public long TotalSpace { get; set; }
    
    /// <summary>
    /// Available free space in bytes
    /// </summary>
    public long FreeSpace { get; set; }
    
    /// <summary>
    /// Used disk space in bytes
    /// </summary>
    public long UsedSpace => TotalSpace - FreeSpace;
    
    /// <summary>
    /// Disk usage percentage (0-100)
    /// </summary>
    public double UsagePercent => TotalSpace > 0 
        ? (double)UsedSpace / TotalSpace * 100 
        : 0;
    
    /// <summary>
    /// Whether the drive is ready for access
    /// </summary>
    public bool IsReady { get; set; }
}