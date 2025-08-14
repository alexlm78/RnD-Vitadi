namespace ImageProcessor.Api.Models;

/// <summary>
/// Request model for image resizing operations
/// </summary>
public class ImageResizeRequest
{
    public string ImagePath { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public bool MaintainAspectRatio { get; set; } = true;
    public string JobId { get; set; } = string.Empty;
}

/// <summary>
/// Request model for applying filters to images
/// </summary>
public class ImageFilterRequest
{
    public string ImagePath { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;
    public List<FilterType> Filters { get; set; } = new();
    public string JobId { get; set; } = string.Empty;
}

/// <summary>
/// Request model for thumbnail generation
/// </summary>
public class ThumbnailRequest
{
    public string ImagePath { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;
    public int Size { get; set; } = 150; // Default thumbnail size
    public string JobId { get; set; } = string.Empty;
}

/// <summary>
/// Request model for batch processing
/// </summary>
public class BatchProcessingRequest
{
    public List<string> ImagePaths { get; set; } = new();
    public string OutputDirectory { get; set; } = string.Empty;
    public ProcessingOperation Operation { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
    public string JobId { get; set; } = string.Empty;
}

/// <summary>
/// Available filter types for image processing
/// </summary>
public enum FilterType
{
    Grayscale,
    Sepia,
    Blur,
    Sharpen,
    Brightness,
    Contrast,
    Invert
}

/// <summary>
/// Available processing operations for batch processing
/// </summary>
public enum ProcessingOperation
{
    Resize,
    ApplyFilters,
    GenerateThumbnails,
    Convert
}

/// <summary>
/// Response model for image processing operations
/// </summary>
public class ImageProcessingResponse
{
    public string JobId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? OutputPath { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? CompletionTime { get; set; }
    public TimeSpan? ProcessingDuration { get; set; }
}

/// <summary>
/// Model for image upload requests
/// </summary>
public class ImageUploadRequest
{
    public IFormFile File { get; set; } = null!;
    public ProcessingOperation? Operation { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
}