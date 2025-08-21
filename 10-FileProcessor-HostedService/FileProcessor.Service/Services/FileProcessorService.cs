using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using FileProcessor.Service.Configuration;
using System.Text;

namespace FileProcessor.Service.Services;

/// <summary>
/// Background service that monitors a directory for files and processes them automatically
/// </summary>
public class FileProcessorService : BackgroundService
{
    private readonly ILogger<FileProcessorService> _logger;
    private readonly FileProcessorOptions _options;
    private readonly FileSystemWatcher _fileWatcher;
    private readonly SemaphoreSlim _processingSemaphore;

    public FileProcessorService(
        ILogger<FileProcessorService> logger,
        IOptions<FileProcessorOptions> options)
    {
        _logger = logger;
        _options = options.Value;
        _processingSemaphore = new SemaphoreSlim(1, 1); // Ensure only one file is processed at a time
        
        // Initialize file system watcher
        _fileWatcher = new FileSystemWatcher();
        ConfigureFileWatcher();
    }

    /// <summary>
    /// Configures the FileSystemWatcher for monitoring the input directory
    /// </summary>
    private void ConfigureFileWatcher()
    {
        try
        {
            // Ensure input directory exists
            if (!Directory.Exists(_options.InputDirectory))
            {
                Directory.CreateDirectory(_options.InputDirectory);
                _logger.LogInformation("Created input directory: {InputDirectory}", _options.InputDirectory);
            }

            // Ensure output directory exists
            if (!Directory.Exists(_options.OutputDirectory))
            {
                Directory.CreateDirectory(_options.OutputDirectory);
                _logger.LogInformation("Created output directory: {OutputDirectory}", _options.OutputDirectory);
            }

            _fileWatcher.Path = _options.InputDirectory;
            _fileWatcher.Filter = "*.*";
            _fileWatcher.NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.FileName;
            _fileWatcher.IncludeSubdirectories = false;

            // Subscribe to events
            _fileWatcher.Created += OnFileCreated;
            _fileWatcher.Changed += OnFileChanged;
            _fileWatcher.Error += OnWatcherError;

            _logger.LogInformation("File watcher configured for directory: {InputDirectory}", _options.InputDirectory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to configure file watcher for directory: {InputDirectory}", _options.InputDirectory);
            throw;
        }
    }

    /// <summary>
    /// Main execution method for the background service
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("FileProcessorService starting...");
        
        try
        {
            // Start file system watcher
            _fileWatcher.EnableRaisingEvents = true;
            _logger.LogInformation("File system watcher started");

            // Process any existing files in the directory
            await ProcessExistingFilesAsync(stoppingToken);

            // Keep the service running and periodically check for files
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(_options.ProcessingIntervalSeconds), stoppingToken);
                    
                    // Periodic check for any missed files
                    await ProcessExistingFilesAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation is requested
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in periodic file processing");
                    // Continue running despite errors
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Critical error in FileProcessorService");
            throw;
        }
        finally
        {
            _fileWatcher.EnableRaisingEvents = false;
            _logger.LogInformation("FileProcessorService stopped");
        }
    }

    /// <summary>
    /// Handles file created events from FileSystemWatcher
    /// </summary>
    private void OnFileCreated(object sender, FileSystemEventArgs e)
    {
        _logger.LogDebug("File created: {FilePath}", e.FullPath);
        // Fire and forget - process file asynchronously
        _ = Task.Run(async () => await ProcessFileAsync(e.FullPath));
    }

    /// <summary>
    /// Handles file changed events from FileSystemWatcher
    /// </summary>
    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        _logger.LogDebug("File changed: {FilePath}", e.FullPath);
        // Fire and forget - process file asynchronously
        _ = Task.Run(async () => await ProcessFileAsync(e.FullPath));
    }

    /// <summary>
    /// Handles FileSystemWatcher errors
    /// </summary>
    private void OnWatcherError(object sender, ErrorEventArgs e)
    {
        _logger.LogError(e.GetException(), "FileSystemWatcher error occurred");
        
        // Try to restart the watcher (fire and forget)
        _ = Task.Run(async () =>
        {
            try
            {
                _fileWatcher.EnableRaisingEvents = false;
                await Task.Delay(1000); // Wait a bit before restarting
                _fileWatcher.EnableRaisingEvents = true;
                _logger.LogInformation("FileSystemWatcher restarted after error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to restart FileSystemWatcher");
            }
        });
    }

    /// <summary>
    /// Processes any existing files in the input directory
    /// </summary>
    private async Task ProcessExistingFilesAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (!Directory.Exists(_options.InputDirectory))
                return;

            var files = Directory.GetFiles(_options.InputDirectory);
            
            if (files.Length > 0)
            {
                _logger.LogInformation("Found {FileCount} existing files to process", files.Length);
                
                foreach (var file in files)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;
                        
                    await ProcessFileAsync(file);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing existing files");
        }
    }

    /// <summary>
    /// Processes a single file
    /// </summary>
    private async Task ProcessFileAsync(string filePath)
    {
        // Use semaphore to ensure only one file is processed at a time
        await _processingSemaphore.WaitAsync();
        
        try
        {
            // Check if file still exists (might have been deleted)
            if (!File.Exists(filePath))
            {
                _logger.LogDebug("File no longer exists: {FilePath}", filePath);
                return;
            }

            var fileName = Path.GetFileName(filePath);
            var fileExtension = Path.GetExtension(filePath).ToLowerInvariant();

            // Check if file extension is supported
            if (!_options.SupportedExtensions.Contains(fileExtension))
            {
                _logger.LogWarning("Unsupported file extension: {FileName} ({Extension})", fileName, fileExtension);
                return;
            }

            // Check file size
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length > _options.MaxFileSizeBytes)
            {
                _logger.LogWarning("File too large: {FileName} ({Size} bytes, max: {MaxSize} bytes)", 
                    fileName, fileInfo.Length, _options.MaxFileSizeBytes);
                return;
            }

            // Wait for file to be completely written (avoid processing partial files)
            await WaitForFileToBeReady(filePath);

            _logger.LogInformation("Processing file: {FileName} ({Size} bytes)", fileName, fileInfo.Length);

            // Process the file based on its type
            var outputPath = await ProcessFileByTypeAsync(filePath, fileExtension);

            _logger.LogInformation("Successfully processed file: {FileName} -> {OutputPath}", fileName, outputPath);

            // Delete the original file after successful processing
            File.Delete(filePath);
            _logger.LogDebug("Deleted original file: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing file: {FilePath}", filePath);
            
            // Move failed file to error directory
            await MoveFileToErrorDirectory(filePath);
        }
        finally
        {
            _processingSemaphore.Release();
        }
    }

    /// <summary>
    /// Waits for a file to be ready for processing (not being written to)
    /// </summary>
    private async Task WaitForFileToBeReady(string filePath)
    {
        const int maxAttempts = 10;
        const int delayMs = 500;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            try
            {
                // Try to open the file exclusively
                using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                return; // File is ready
            }
            catch (IOException)
            {
                // File is still being written to
                if (attempt == maxAttempts - 1)
                {
                    _logger.LogWarning("File may still be in use after {Attempts} attempts: {FilePath}", maxAttempts, filePath);
                    throw;
                }
                
                await Task.Delay(delayMs);
            }
        }
    }

    /// <summary>
    /// Processes a file based on its type/extension
    /// </summary>
    private async Task<string> ProcessFileByTypeAsync(string filePath, string extension)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var outputFileName = $"{fileName}_processed_{timestamp}{extension}";
        var outputPath = Path.Combine(_options.OutputDirectory, outputFileName);

        switch (extension)
        {
            case ".txt":
                await ProcessTextFileAsync(filePath, outputPath);
                break;
            case ".csv":
                await ProcessCsvFileAsync(filePath, outputPath);
                break;
            case ".json":
                await ProcessJsonFileAsync(filePath, outputPath);
                break;
            case ".xml":
                await ProcessXmlFileAsync(filePath, outputPath);
                break;
            default:
                // Generic processing - just copy with metadata
                await ProcessGenericFileAsync(filePath, outputPath);
                break;
        }

        return outputPath;
    }

    /// <summary>
    /// Processes text files - adds line numbers and metadata
    /// </summary>
    private async Task ProcessTextFileAsync(string inputPath, string outputPath)
    {
        var lines = await File.ReadAllLinesAsync(inputPath);
        var processedLines = new List<string>
        {
            $"# Processed on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
            $"# Original file: {Path.GetFileName(inputPath)}",
            $"# Total lines: {lines.Length}",
            ""
        };

        for (int i = 0; i < lines.Length; i++)
        {
            processedLines.Add($"{i + 1:D4}: {lines[i]}");
        }

        await File.WriteAllLinesAsync(outputPath, processedLines);
    }

    /// <summary>
    /// Processes CSV files - adds row count and validation
    /// </summary>
    private async Task ProcessCsvFileAsync(string inputPath, string outputPath)
    {
        var lines = await File.ReadAllLinesAsync(inputPath);
        var processedLines = new List<string>
        {
            $"# CSV Processed on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
            $"# Original file: {Path.GetFileName(inputPath)}",
            $"# Total rows: {lines.Length}",
            ""
        };

        // Add original content with row numbers
        for (int i = 0; i < lines.Length; i++)
        {
            var prefix = i == 0 ? "HEADER" : $"ROW_{i:D4}";
            processedLines.Add($"{prefix},{lines[i]}");
        }

        await File.WriteAllLinesAsync(outputPath, processedLines);
    }

    /// <summary>
    /// Processes JSON files - validates and adds metadata
    /// </summary>
    private async Task ProcessJsonFileAsync(string inputPath, string outputPath)
    {
        var content = await File.ReadAllTextAsync(inputPath);
        
        // Basic JSON validation
        try
        {
            System.Text.Json.JsonDocument.Parse(content);
        }
        catch (System.Text.Json.JsonException ex)
        {
            throw new InvalidOperationException($"Invalid JSON format: {ex.Message}");
        }

        var metadata = new
        {
            ProcessedOn = DateTime.Now,
            OriginalFile = Path.GetFileName(inputPath),
            FileSize = new FileInfo(inputPath).Length,
            IsValidJson = true
        };

        var processedContent = new
        {
            Metadata = metadata,
            OriginalContent = System.Text.Json.JsonDocument.Parse(content).RootElement
        };

        var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
        var json = System.Text.Json.JsonSerializer.Serialize(processedContent, options);
        
        await File.WriteAllTextAsync(outputPath, json);
    }

    /// <summary>
    /// Processes XML files - validates and adds metadata
    /// </summary>
    private async Task ProcessXmlFileAsync(string inputPath, string outputPath)
    {
        var content = await File.ReadAllTextAsync(inputPath);
        
        // Basic XML validation
        try
        {
            var doc = new System.Xml.XmlDocument();
            doc.LoadXml(content);
        }
        catch (System.Xml.XmlException ex)
        {
            throw new InvalidOperationException($"Invalid XML format: {ex.Message}");
        }

        var processedContent = new StringBuilder();
        processedContent.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        processedContent.AppendLine("<ProcessedDocument>");
        processedContent.AppendLine("  <Metadata>");
        processedContent.AppendLine($"    <ProcessedOn>{DateTime.Now:yyyy-MM-dd HH:mm:ss}</ProcessedOn>");
        processedContent.AppendLine($"    <OriginalFile>{Path.GetFileName(inputPath)}</OriginalFile>");
        processedContent.AppendLine($"    <FileSize>{new FileInfo(inputPath).Length}</FileSize>");
        processedContent.AppendLine("    <IsValidXml>true</IsValidXml>");
        processedContent.AppendLine("  </Metadata>");
        processedContent.AppendLine("  <OriginalContent>");
        processedContent.AppendLine(content);
        processedContent.AppendLine("  </OriginalContent>");
        processedContent.AppendLine("</ProcessedDocument>");

        await File.WriteAllTextAsync(outputPath, processedContent.ToString());
    }

    /// <summary>
    /// Generic file processing - copies file with metadata
    /// </summary>
    private async Task ProcessGenericFileAsync(string inputPath, string outputPath)
    {
        // For generic files, just copy and add a metadata file
        File.Copy(inputPath, outputPath);
        
        var metadataPath = outputPath + ".metadata";
        var metadata = new StringBuilder();
        metadata.AppendLine($"Processed on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        metadata.AppendLine($"Original file: {Path.GetFileName(inputPath)}");
        metadata.AppendLine($"File size: {new FileInfo(inputPath).Length} bytes");
        metadata.AppendLine($"Processing type: Generic copy");
        
        await File.WriteAllTextAsync(metadataPath, metadata.ToString());
    }

    /// <summary>
    /// Moves a file to the error directory when processing fails
    /// </summary>
    private Task MoveFileToErrorDirectory(string filePath)
    {
        try
        {
            var errorDirectory = Path.Combine(_options.OutputDirectory, "errors");
            if (!Directory.Exists(errorDirectory))
            {
                Directory.CreateDirectory(errorDirectory);
            }

            var fileName = Path.GetFileName(filePath);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var errorFileName = $"{Path.GetFileNameWithoutExtension(fileName)}_error_{timestamp}{Path.GetExtension(fileName)}";
            var errorPath = Path.Combine(errorDirectory, errorFileName);

            File.Move(filePath, errorPath);
            _logger.LogWarning("Moved failed file to error directory: {ErrorPath}", errorPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to move file to error directory: {FilePath}", filePath);
        }
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Cleanup resources when the service is disposed
    /// </summary>
    public override void Dispose()
    {
        _fileWatcher?.Dispose();
        _processingSemaphore?.Dispose();
        base.Dispose();
    }
}