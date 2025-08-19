using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthDashboard.Api.HealthChecks;

/// <summary>
/// Custom health check for file system operations
/// Demonstrates how to check file system dependencies
/// </summary>
public class FileSystemHealthCheck : IHealthCheck
{
    private readonly ILogger<FileSystemHealthCheck> _logger;
    private readonly string _directoryPath;
    private readonly long _minFreeSpaceBytes;

    public FileSystemHealthCheck(ILogger<FileSystemHealthCheck> logger, string directoryPath, long minFreeSpaceBytes = 1024 * 1024 * 100) // 100MB default
    {
        _logger = logger;
        _directoryPath = directoryPath;
        _minFreeSpaceBytes = minFreeSpaceBytes;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Checking file system health: {DirectoryPath}", _directoryPath);

            // Check if directory exists
            if (!Directory.Exists(_directoryPath))
            {
                _logger.LogError("Directory does not exist: {DirectoryPath}", _directoryPath);
                return Task.FromResult(HealthCheckResult.Unhealthy($"Directory does not exist: {_directoryPath}"));
            }

            // Check if directory is writable
            var testFilePath = Path.Combine(_directoryPath, $"health_check_test_{Guid.NewGuid()}.tmp");
            try
            {
                File.WriteAllText(testFilePath, "health check test");
                File.Delete(testFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Directory is not writable: {DirectoryPath}", _directoryPath);
                return Task.FromResult(HealthCheckResult.Unhealthy($"Directory is not writable: {_directoryPath}", ex));
            }

            // Check available disk space
            var driveInfo = new DriveInfo(Path.GetPathRoot(_directoryPath) ?? _directoryPath);
            var availableSpace = driveInfo.AvailableFreeSpace;
            var totalSpace = driveInfo.TotalSize;
            var usedSpacePercentage = ((double)(totalSpace - availableSpace) / totalSpace) * 100;

            var data = new Dictionary<string, object>
            {
                ["directory_path"] = _directoryPath,
                ["available_space_bytes"] = availableSpace,
                ["total_space_bytes"] = totalSpace,
                ["used_space_percentage"] = Math.Round(usedSpacePercentage, 2),
                ["min_required_space_bytes"] = _minFreeSpaceBytes
            };

            if (availableSpace < _minFreeSpaceBytes)
            {
                _logger.LogWarning("Low disk space: {AvailableSpace} bytes available, minimum required: {MinRequired} bytes", 
                    availableSpace, _minFreeSpaceBytes);
                return Task.FromResult(HealthCheckResult.Degraded(
                    $"Low disk space: {availableSpace / (1024 * 1024)}MB available, minimum required: {_minFreeSpaceBytes / (1024 * 1024)}MB", 
                    data: data));
            }

            _logger.LogInformation("File system is healthy: {DirectoryPath}, {AvailableSpace}MB available", 
                _directoryPath, availableSpace / (1024 * 1024));
            
            return Task.FromResult(HealthCheckResult.Healthy(
                $"File system is healthy. Available space: {availableSpace / (1024 * 1024)}MB", 
                data: data));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "File system health check failed: {DirectoryPath}", _directoryPath);
            return Task.FromResult(HealthCheckResult.Unhealthy($"File system health check failed: {ex.Message}", ex));
        }
    }
}