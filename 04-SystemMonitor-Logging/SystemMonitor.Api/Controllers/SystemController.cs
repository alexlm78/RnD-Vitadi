using Microsoft.AspNetCore.Mvc;
using SystemMonitor.Api.Models;
using SystemMonitor.Api.Services;

namespace SystemMonitor.Api.Controllers;

/// <summary>
/// Controller for system monitoring and metrics endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class SystemController : ControllerBase
{
    private readonly ISystemMetricsService _systemMetricsService;
    private readonly ILogger<SystemController> _logger;

    public SystemController(ISystemMetricsService systemMetricsService, ILogger<SystemController> logger)
    {
        _systemMetricsService = systemMetricsService;
        _logger = logger;
    }

    /// <summary>
    /// Gets comprehensive system metrics including CPU, memory, and disk usage
    /// </summary>
    /// <returns>Current system metrics</returns>
    /// <response code="200">Returns the current system metrics</response>
    /// <response code="500">If there was an error collecting metrics</response>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(SystemMetrics), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SystemMetrics>> GetSystemMetrics()
    {
        using var activity = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = "GetSystemMetrics",
            ["RequestId"] = HttpContext.TraceIdentifier,
            ["UserAgent"] = HttpContext.Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown"
        });

        try
        {
            _logger.LogInformation("System metrics collection started");
            var startTime = DateTime.UtcNow;
            
            var metrics = await _systemMetricsService.GetSystemMetricsAsync();
            
            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation("System metrics collected successfully in {Duration}ms. CPU: {CpuUsage:F1}%, Memory: {MemoryUsage:F1}%, Disks: {DiskCount}",
                duration.TotalMilliseconds,
                metrics.CpuUsagePercent,
                metrics.Memory.UsagePercent,
                metrics.Disks.Count);
            
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve system metrics");
            return StatusCode(500, new { error = "Failed to retrieve system metrics", details = ex.Message });
        }
    }

    /// <summary>
    /// Gets current CPU usage percentage
    /// </summary>
    /// <returns>CPU usage as percentage (0-100)</returns>
    /// <response code="200">Returns the current CPU usage</response>
    /// <response code="500">If there was an error collecting CPU metrics</response>
    [HttpGet("cpu")]
    [ProducesResponseType(typeof(double), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<double>> GetCpuUsage()
    {
        using var activity = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = "GetCpuUsage",
            ["RequestId"] = HttpContext.TraceIdentifier
        });

        try
        {
            _logger.LogDebug("CPU usage collection started");
            var cpuUsage = await _systemMetricsService.GetCpuUsageAsync();
            
            _logger.LogInformation("CPU usage collected: {CpuUsage:F1}%", cpuUsage);
            return Ok(cpuUsage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve CPU usage");
            return StatusCode(500, new { error = "Failed to retrieve CPU usage", details = ex.Message });
        }
    }

    /// <summary>
    /// Gets current memory usage information
    /// </summary>
    /// <returns>Memory usage details</returns>
    /// <response code="200">Returns the current memory usage</response>
    /// <response code="500">If there was an error collecting memory metrics</response>
    [HttpGet("memory")]
    [ProducesResponseType(typeof(MemoryMetrics), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MemoryMetrics>> GetMemoryUsage()
    {
        using var activity = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = "GetMemoryUsage",
            ["RequestId"] = HttpContext.TraceIdentifier
        });

        try
        {
            _logger.LogDebug("Memory usage collection started");
            var memoryUsage = await _systemMetricsService.GetMemoryUsageAsync();
            
            _logger.LogInformation("Memory usage collected: {MemoryUsage:F1}% ({UsedMemory:N0} / {TotalMemory:N0} bytes)",
                memoryUsage.UsagePercent,
                memoryUsage.UsedPhysicalMemory,
                memoryUsage.TotalPhysicalMemory);
            
            return Ok(memoryUsage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve memory usage");
            return StatusCode(500, new { error = "Failed to retrieve memory usage", details = ex.Message });
        }
    }

    /// <summary>
    /// Gets disk usage information for all drives
    /// </summary>
    /// <returns>List of disk usage information</returns>
    /// <response code="200">Returns disk usage for all drives</response>
    /// <response code="500">If there was an error collecting disk metrics</response>
    [HttpGet("disk")]
    [ProducesResponseType(typeof(List<DiskMetrics>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<DiskMetrics>>> GetDiskUsage()
    {
        using var activity = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = "GetDiskUsage",
            ["RequestId"] = HttpContext.TraceIdentifier
        });

        try
        {
            _logger.LogDebug("Disk usage collection started");
            var diskUsage = await _systemMetricsService.GetDiskUsageAsync();
            
            var readyDisks = diskUsage.Where(d => d.IsReady).ToList();
            _logger.LogInformation("Disk usage collected for {DiskCount} drives. Ready drives: {ReadyDiskCount}",
                diskUsage.Count,
                readyDisks.Count);
            
            foreach (var disk in readyDisks)
            {
                _logger.LogDebug("Drive {DriveName}: {UsagePercent:F1}% used ({UsedSpace:N0} / {TotalSpace:N0} bytes)",
                    disk.DriveName,
                    disk.UsagePercent,
                    disk.UsedSpace,
                    disk.TotalSpace);
            }
            
            return Ok(diskUsage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve disk usage");
            return StatusCode(500, new { error = "Failed to retrieve disk usage", details = ex.Message });
        }
    }

    /// <summary>
    /// Gets system information summary
    /// </summary>
    /// <returns>Basic system information</returns>
    /// <response code="200">Returns system information</response>
    [HttpGet("info")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetSystemInfo()
    {
        using var activity = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = "GetSystemInfo",
            ["RequestId"] = HttpContext.TraceIdentifier
        });

        try
        {
            var info = new
            {
                MachineName = Environment.MachineName,
                OSVersion = Environment.OSVersion.ToString(),
                ProcessorCount = Environment.ProcessorCount,
                Is64BitOperatingSystem = Environment.Is64BitOperatingSystem,
                Is64BitProcess = Environment.Is64BitProcess,
                CLRVersion = Environment.Version.ToString(),
                WorkingSet = Environment.WorkingSet,
                SystemPageSize = Environment.SystemPageSize,
                TickCount = Environment.TickCount64,
                UserName = Environment.UserName,
                UserDomainName = Environment.UserDomainName,
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("System info collected for machine {MachineName} running {OSVersion} with {ProcessorCount} processors",
                info.MachineName,
                info.OSVersion,
                info.ProcessorCount);
            
            return Ok(info);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve system information");
            return StatusCode(500, new { error = "Failed to retrieve system information", details = ex.Message });
        }
    }

    /// <summary>
    /// Gets application health status and uptime information
    /// </summary>
    /// <returns>Application health and uptime details</returns>
    /// <response code="200">Returns application health status</response>
    [HttpGet("health")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetHealthStatus()
    {
        using var activity = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = "GetHealthStatus",
            ["RequestId"] = HttpContext.TraceIdentifier
        });

        try
        {
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var startTime = process.StartTime;
            var uptime = DateTime.Now - startTime;
            
            var health = new
            {
                Status = "Healthy",
                Uptime = new
                {
                    Days = uptime.Days,
                    Hours = uptime.Hours,
                    Minutes = uptime.Minutes,
                    Seconds = uptime.Seconds,
                    TotalMilliseconds = uptime.TotalMilliseconds
                },
                StartTime = startTime,
                CurrentTime = DateTime.UtcNow,
                ProcessId = process.Id,
                ProcessName = process.ProcessName,
                ThreadCount = process.Threads.Count,
                HandleCount = process.HandleCount,
                GCMemory = GC.GetTotalMemory(false),
                GCCollections = new
                {
                    Gen0 = GC.CollectionCount(0),
                    Gen1 = GC.CollectionCount(1),
                    Gen2 = GC.CollectionCount(2)
                }
            };

            _logger.LogInformation("Health status checked. Uptime: {UptimeDays}d {UptimeHours}h {UptimeMinutes}m, Threads: {ThreadCount}, GC Memory: {GCMemory:N0} bytes",
                uptime.Days,
                uptime.Hours,
                uptime.Minutes,
                health.ThreadCount,
                health.GCMemory);
            
            return Ok(health);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve health status");
            return StatusCode(500, new { error = "Failed to retrieve health status", details = ex.Message });
        }
    }

    /// <summary>
    /// Gets detailed process information and performance counters
    /// </summary>
    /// <returns>Process performance details</returns>
    /// <response code="200">Returns process performance information</response>
    [HttpGet("process")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetProcessInfo()
    {
        using var activity = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = "GetProcessInfo",
            ["RequestId"] = HttpContext.TraceIdentifier
        });

        try
        {
            var process = System.Diagnostics.Process.GetCurrentProcess();
            
            var processInfo = new
            {
                ProcessId = process.Id,
                ProcessName = process.ProcessName,
                StartTime = process.StartTime,
                TotalProcessorTime = process.TotalProcessorTime,
                UserProcessorTime = process.UserProcessorTime,
                PrivilegedProcessorTime = process.PrivilegedProcessorTime,
                WorkingSet64 = process.WorkingSet64,
                VirtualMemorySize64 = process.VirtualMemorySize64,
                PrivateMemorySize64 = process.PrivateMemorySize64,
                PagedMemorySize64 = process.PagedMemorySize64,
                PagedSystemMemorySize64 = process.PagedSystemMemorySize64,
                NonpagedSystemMemorySize64 = process.NonpagedSystemMemorySize64,
                PeakWorkingSet64 = process.PeakWorkingSet64,
                PeakVirtualMemorySize64 = process.PeakVirtualMemorySize64,
                PeakPagedMemorySize64 = process.PeakPagedMemorySize64,
                ThreadCount = process.Threads.Count,
                HandleCount = process.HandleCount,
                BasePriority = process.BasePriority,
                PriorityClass = process.PriorityClass.ToString(),
                Responding = process.Responding,
                SessionId = process.SessionId,
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Process info collected for PID {ProcessId}. Working Set: {WorkingSet:N0} bytes, Threads: {ThreadCount}, Handles: {HandleCount}",
                processInfo.ProcessId,
                processInfo.WorkingSet64,
                processInfo.ThreadCount,
                processInfo.HandleCount);
            
            return Ok(processInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve process information");
            return StatusCode(500, new { error = "Failed to retrieve process information", details = ex.Message });
        }
    }

    /// <summary>
    /// Gets environment variables and configuration information
    /// </summary>
    /// <returns>Environment and configuration details</returns>
    /// <response code="200">Returns environment information</response>
    [HttpGet("environment")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult GetEnvironmentInfo()
    {
        using var activity = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = "GetEnvironmentInfo",
            ["RequestId"] = HttpContext.TraceIdentifier
        });

        try
        {
            var environmentInfo = new
            {
                MachineName = Environment.MachineName,
                OSVersion = Environment.OSVersion.ToString(),
                Platform = Environment.OSVersion.Platform.ToString(),
                ProcessorCount = Environment.ProcessorCount,
                Is64BitOperatingSystem = Environment.Is64BitOperatingSystem,
                Is64BitProcess = Environment.Is64BitProcess,
                CLRVersion = Environment.Version.ToString(),
                CurrentDirectory = Environment.CurrentDirectory,
                SystemDirectory = Environment.SystemDirectory,
                UserName = Environment.UserName,
                UserDomainName = Environment.UserDomainName,
                UserInteractive = Environment.UserInteractive,
                SystemPageSize = Environment.SystemPageSize,
                TickCount = Environment.TickCount64,
                HasShutdownStarted = Environment.HasShutdownStarted,
                CommandLine = Environment.CommandLine,
                ProcessPath = Environment.ProcessPath,
                Timestamp = DateTime.UtcNow,
                // Filter sensitive environment variables
                EnvironmentVariables = Environment.GetEnvironmentVariables()
                    .Cast<System.Collections.DictionaryEntry>()
                    .Where(entry => entry.Key != null && !IsSensitiveEnvironmentVariable(entry.Key.ToString()!))
                    .ToDictionary(entry => entry.Key.ToString()!, entry => entry.Value?.ToString() ?? string.Empty)
            };

            _logger.LogInformation("Environment info collected for {MachineName}. Platform: {Platform}, CLR: {CLRVersion}, Processor Count: {ProcessorCount}",
                environmentInfo.MachineName,
                environmentInfo.Platform,
                environmentInfo.CLRVersion,
                environmentInfo.ProcessorCount);
            
            return Ok(environmentInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve environment information");
            return StatusCode(500, new { error = "Failed to retrieve environment information", details = ex.Message });
        }
    }

    /// <summary>
    /// Triggers garbage collection and returns memory statistics
    /// </summary>
    /// <returns>Memory statistics before and after GC</returns>
    /// <response code="200">Returns GC statistics</response>
    [HttpPost("gc")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult TriggerGarbageCollection()
    {
        using var activity = _logger.BeginScope(new Dictionary<string, object>
        {
            ["Operation"] = "TriggerGarbageCollection",
            ["RequestId"] = HttpContext.TraceIdentifier
        });

        try
        {
            var beforeGC = new
            {
                TotalMemory = GC.GetTotalMemory(false),
                Gen0Collections = GC.CollectionCount(0),
                Gen1Collections = GC.CollectionCount(1),
                Gen2Collections = GC.CollectionCount(2)
            };

            _logger.LogInformation("Triggering garbage collection. Memory before GC: {MemoryBefore:N0} bytes", beforeGC.TotalMemory);
            
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            stopwatch.Stop();

            var afterGC = new
            {
                TotalMemory = GC.GetTotalMemory(false),
                Gen0Collections = GC.CollectionCount(0),
                Gen1Collections = GC.CollectionCount(1),
                Gen2Collections = GC.CollectionCount(2)
            };

            var result = new
            {
                Before = beforeGC,
                After = afterGC,
                MemoryFreed = beforeGC.TotalMemory - afterGC.TotalMemory,
                DurationMs = stopwatch.ElapsedMilliseconds,
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Garbage collection completed in {Duration}ms. Memory freed: {MemoryFreed:N0} bytes ({MemoryBefore:N0} -> {MemoryAfter:N0})",
                result.DurationMs,
                result.MemoryFreed,
                beforeGC.TotalMemory,
                afterGC.TotalMemory);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to trigger garbage collection");
            return StatusCode(500, new { error = "Failed to trigger garbage collection", details = ex.Message });
        }
    }

    private static bool IsSensitiveEnvironmentVariable(string key)
    {
        var sensitiveKeys = new[]
        {
            "PASSWORD", "SECRET", "KEY", "TOKEN", "CONNECTIONSTRING", 
            "APIKEY", "API_KEY", "AUTH", "CREDENTIAL", "PWD"
        };
        
        return sensitiveKeys.Any(sensitive => 
            key.ToUpperInvariant().Contains(sensitive));
    }
}