using ImageProcessor.Api.Models;

namespace ImageProcessor.Api.Services;

/// <summary>
/// Interface for image processing operations
/// </summary>
public interface IImageProcessingService
{
    /// <summary>
    /// Resize an image to specified dimensions
    /// </summary>
    /// <param name="request">Resize request parameters</param>
    /// <returns>Task representing the async operation</returns>
    Task ResizeImageAsync(ImageResizeRequest request);

    /// <summary>
    /// Apply filters to an image
    /// </summary>
    /// <param name="request">Filter request parameters</param>
    /// <returns>Task representing the async operation</returns>
    Task ApplyFiltersAsync(ImageFilterRequest request);

    /// <summary>
    /// Generate thumbnail for an image
    /// </summary>
    /// <param name="request">Thumbnail request parameters</param>
    /// <returns>Task representing the async operation</returns>
    Task GenerateThumbnailAsync(ThumbnailRequest request);

    /// <summary>
    /// Process multiple images in batch
    /// </summary>
    /// <param name="request">Batch processing request</param>
    /// <returns>Task representing the async operation</returns>
    Task ProcessBatchAsync(BatchProcessingRequest request);

    /// <summary>
    /// Clean up old processed images (recurring job)
    /// </summary>
    /// <returns>Task representing the async operation</returns>
    Task CleanupOldImagesAsync();
}