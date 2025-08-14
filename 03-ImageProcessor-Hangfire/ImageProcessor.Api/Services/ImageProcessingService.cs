using ImageProcessor.Api.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;

namespace ImageProcessor.Api.Services;

/// <summary>
/// Service for processing images using ImageSharp library
/// </summary>
public class ImageProcessingService : IImageProcessingService
{
    private readonly ILogger<ImageProcessingService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _uploadPath;
    private readonly string _processedPath;

    public ImageProcessingService(ILogger<ImageProcessingService> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        
        // Configure paths for image storage
        _uploadPath = _configuration.GetValue<string>("ImageProcessing:UploadPath") ?? "uploads";
        _processedPath = _configuration.GetValue<string>("ImageProcessing:ProcessedPath") ?? "processed";
        
        // Ensure directories exist
        Directory.CreateDirectory(_uploadPath);
        Directory.CreateDirectory(_processedPath);
    }

    /// <summary>
    /// Resize an image to specified dimensions
    /// </summary>
    public async Task ResizeImageAsync(ImageResizeRequest request)
    {
        _logger.LogInformation("Starting image resize operation for job {JobId}", request.JobId);
        
        try
        {
            using var image = await Image.LoadAsync(request.ImagePath);
            
            var resizeOptions = new ResizeOptions
            {
                Size = new Size(request.Width, request.Height),
                Mode = request.MaintainAspectRatio ? ResizeMode.Max : ResizeMode.Stretch
            };

            image.Mutate(x => x.Resize(resizeOptions));
            
            await image.SaveAsync(request.OutputPath, new JpegEncoder { Quality = 90 });
            
            _logger.LogInformation("Successfully resized image for job {JobId}. Output: {OutputPath}", 
                request.JobId, request.OutputPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resizing image for job {JobId}", request.JobId);
            throw;
        }
    }

    /// <summary>
    /// Apply various filters to an image
    /// </summary>
    public async Task ApplyFiltersAsync(ImageFilterRequest request)
    {
        _logger.LogInformation("Starting filter application for job {JobId} with {FilterCount} filters", 
            request.JobId, request.Filters.Count);
        
        try
        {
            using var image = await Image.LoadAsync(request.ImagePath);
            
            image.Mutate(ctx =>
            {
                foreach (var filter in request.Filters)
                {
                    ApplyFilter(ctx, filter);
                }
            });
            
            await image.SaveAsync(request.OutputPath, new JpegEncoder { Quality = 90 });
            
            _logger.LogInformation("Successfully applied filters for job {JobId}. Output: {OutputPath}", 
                request.JobId, request.OutputPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying filters for job {JobId}", request.JobId);
            throw;
        }
    }

    /// <summary>
    /// Generate a thumbnail for an image
    /// </summary>
    public async Task GenerateThumbnailAsync(ThumbnailRequest request)
    {
        _logger.LogInformation("Starting thumbnail generation for job {JobId} with size {Size}", 
            request.JobId, request.Size);
        
        try
        {
            using var image = await Image.LoadAsync(request.ImagePath);
            
            var resizeOptions = new ResizeOptions
            {
                Size = new Size(request.Size, request.Size),
                Mode = ResizeMode.Crop
            };

            image.Mutate(x => x.Resize(resizeOptions));
            
            await image.SaveAsync(request.OutputPath, new JpegEncoder { Quality = 85 });
            
            _logger.LogInformation("Successfully generated thumbnail for job {JobId}. Output: {OutputPath}", 
                request.JobId, request.OutputPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating thumbnail for job {JobId}", request.JobId);
            throw;
        }
    }

    /// <summary>
    /// Process multiple images in batch
    /// </summary>
    public async Task ProcessBatchAsync(BatchProcessingRequest request)
    {
        _logger.LogInformation("Starting batch processing for job {JobId} with {ImageCount} images", 
            request.JobId, request.ImagePaths.Count);
        
        try
        {
            var tasks = new List<Task>();
            
            foreach (var imagePath in request.ImagePaths)
            {
                var fileName = Path.GetFileNameWithoutExtension(imagePath);
                var extension = Path.GetExtension(imagePath);
                var outputPath = Path.Combine(request.OutputDirectory, $"{fileName}_processed{extension}");
                
                var task = request.Operation switch
                {
                    ProcessingOperation.Resize => ProcessSingleImageResize(imagePath, outputPath, request.Parameters),
                    ProcessingOperation.ApplyFilters => ProcessSingleImageFilters(imagePath, outputPath, request.Parameters),
                    ProcessingOperation.GenerateThumbnails => ProcessSingleImageThumbnail(imagePath, outputPath, request.Parameters),
                    _ => Task.CompletedTask
                };
                
                tasks.Add(task);
            }
            
            await Task.WhenAll(tasks);
            
            _logger.LogInformation("Successfully completed batch processing for job {JobId}", request.JobId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in batch processing for job {JobId}", request.JobId);
            throw;
        }
    }

    /// <summary>
    /// Clean up old processed images (recurring job)
    /// </summary>
    public async Task CleanupOldImagesAsync()
    {
        _logger.LogInformation("Starting cleanup of old processed images");
        
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-7); // Keep files for 7 days
            var processedFiles = Directory.GetFiles(_processedPath, "*", SearchOption.AllDirectories);
            
            var deletedCount = 0;
            
            foreach (var file in processedFiles)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.CreationTimeUtc < cutoffDate)
                {
                    File.Delete(file);
                    deletedCount++;
                }
            }
            
            _logger.LogInformation("Cleanup completed. Deleted {DeletedCount} old files", deletedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cleanup operation");
            throw;
        }
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// Apply a specific filter to the image processing context
    /// </summary>
    private static void ApplyFilter(IImageProcessingContext ctx, FilterType filter)
    {
        switch (filter)
        {
            case FilterType.Grayscale:
                ctx.Grayscale();
                break;
            case FilterType.Sepia:
                ctx.Sepia();
                break;
            case FilterType.Blur:
                ctx.GaussianBlur(3f);
                break;
            case FilterType.Sharpen:
                ctx.GaussianSharpen();
                break;
            case FilterType.Brightness:
                ctx.Brightness(1.2f);
                break;
            case FilterType.Contrast:
                ctx.Contrast(1.2f);
                break;
            case FilterType.Invert:
                ctx.Invert();
                break;
        }
    }

    /// <summary>
    /// Process a single image for resize operation
    /// </summary>
    private async Task ProcessSingleImageResize(string imagePath, string outputPath, Dictionary<string, object> parameters)
    {
        var width = parameters.GetValueOrDefault("width", 800);
        var height = parameters.GetValueOrDefault("height", 600);
        var maintainAspectRatio = parameters.GetValueOrDefault("maintainAspectRatio", true);
        
        var request = new ImageResizeRequest
        {
            ImagePath = imagePath,
            OutputPath = outputPath,
            Width = Convert.ToInt32(width),
            Height = Convert.ToInt32(height),
            MaintainAspectRatio = Convert.ToBoolean(maintainAspectRatio),
            JobId = Guid.NewGuid().ToString()
        };
        
        await ResizeImageAsync(request);
    }

    /// <summary>
    /// Process a single image for filter application
    /// </summary>
    private async Task ProcessSingleImageFilters(string imagePath, string outputPath, Dictionary<string, object> parameters)
    {
        var filters = new List<FilterType>();
        
        if (parameters.ContainsKey("filters") && parameters["filters"] is List<FilterType> filterList)
        {
            filters = filterList;
        }
        else if (parameters.ContainsKey("filter") && Enum.TryParse<FilterType>(parameters["filter"].ToString(), out var singleFilter))
        {
            filters.Add(singleFilter);
        }
        
        var request = new ImageFilterRequest
        {
            ImagePath = imagePath,
            OutputPath = outputPath,
            Filters = filters,
            JobId = Guid.NewGuid().ToString()
        };
        
        await ApplyFiltersAsync(request);
    }

    /// <summary>
    /// Process a single image for thumbnail generation
    /// </summary>
    private async Task ProcessSingleImageThumbnail(string imagePath, string outputPath, Dictionary<string, object> parameters)
    {
        var size = parameters.GetValueOrDefault("size", 150);
        
        var request = new ThumbnailRequest
        {
            ImagePath = imagePath,
            OutputPath = outputPath,
            Size = Convert.ToInt32(size),
            JobId = Guid.NewGuid().ToString()
        };
        
        await GenerateThumbnailAsync(request);
    }
}