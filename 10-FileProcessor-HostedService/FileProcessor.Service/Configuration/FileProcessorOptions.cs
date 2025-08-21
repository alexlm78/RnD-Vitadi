namespace FileProcessor.Service.Configuration;

/// <summary>
/// Configuration options for the file processor service
/// </summary>
public class FileProcessorOptions
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "FileProcessor";

    /// <summary>
    /// Directory to monitor for input files
    /// </summary>
    public string InputDirectory { get; set; } = "./input";

    /// <summary>
    /// Directory to place processed files
    /// </summary>
    public string OutputDirectory { get; set; } = "./output";

    /// <summary>
    /// Interval in seconds between processing cycles
    /// </summary>
    public int ProcessingIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Supported file extensions for processing
    /// </summary>
    public string[] SupportedExtensions { get; set; } = [".txt", ".csv", ".json", ".xml"];

    /// <summary>
    /// Maximum file size in bytes that can be processed
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024; // 10 MB
}