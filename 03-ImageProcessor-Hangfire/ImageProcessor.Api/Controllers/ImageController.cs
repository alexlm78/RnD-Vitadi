using Hangfire;
using Microsoft.AspNetCore.Mvc;
using ImageProcessor.Api.Models;
using ImageProcessor.Api.Services;

namespace ImageProcessor.Api.Controllers;

/// <summary>
/// Controller for image processing operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ImageController : ControllerBase
{
    private readonly ILogger<ImageController> _logger;
    private readonly IImageProcessingService _imageProcessingService;
    private readonly IConfiguration _configuration;
    private readonly string _uploadPath;
    private readonly string _processedPath;

    public ImageController(
        ILogger<ImageController> logger, 
        IImageProcessingService imageProcessingService,
        IConfiguration configuration)
    {
        _logger = logger;
        _imageProcessingService = imageProcessingService;
        _configuration = configuration;
        
        _uploadPath = _configuration.GetValue<string>("ImageProcessing:UploadPath") ?? "uploads";
        _processedPath = _configuration.GetValue<string>("ImageProcessing:ProcessedPath") ?? "processed";
        
        // Ensure directories exist
        Directory.CreateDirectory(_uploadPath);
        Directory.CreateDirectory(_processedPath);
    }

    /// <summary>
    /// Health check endpoint for the Image Controller
    /// </summary>
    /// <returns>Status message</returns>
    [HttpGet("health")]
    public IActionResult Health()
    {
        _logger.LogInformation("Image Controller health check requested");
        return Ok(new { Status = "Healthy", Service = "ImageProcessor", Timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Upload and process an image with comprehensive validation and job scheduling
    /// </summary>
    /// <param name="request">Image upload request</param>
    /// <returns>Processing job information</returns>
    [HttpPost("upload")]
    public async Task<IActionResult> UploadImage([FromForm] ImageUploadRequest request)
    {
        if (request.File == null || request.File.Length == 0)
        {
            return BadRequest(new { Error = "No file uploaded", Code = "NO_FILE" });
        }

        // Validate file size
        var maxFileSize = _configuration.GetValue<long>("ImageProcessing:MaxFileSize", 10485760); // 10MB default
        if (request.File.Length > maxFileSize)
        {
            return BadRequest(new { 
                Error = $"File size exceeds maximum allowed size of {maxFileSize / 1024 / 1024}MB", 
                Code = "FILE_TOO_LARGE" 
            });
        }

        // Validate file extension
        var allowedExtensions = _configuration.GetSection("ImageProcessing:AllowedExtensions").Get<string[]>() 
            ?? new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        var fileExtension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(fileExtension))
        {
            return BadRequest(new { 
                Error = $"File type {fileExtension} not allowed. Allowed types: {string.Join(", ", allowedExtensions)}", 
                Code = "INVALID_FILE_TYPE" 
            });
        }

        try
        {
            // Save uploaded file with timestamp and unique identifier
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var uniqueId = Guid.NewGuid().ToString("N")[..8];
            var fileName = $"{timestamp}_{uniqueId}_{Path.GetFileNameWithoutExtension(request.File.FileName)}{fileExtension}";
            var filePath = Path.Combine(_uploadPath, fileName);
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.File.CopyToAsync(stream);
            }

            var jobId = Guid.NewGuid().ToString();
            var outputPath = Path.Combine(_processedPath, $"processed_{fileName}");

            // Schedule background job based on operation type with specific queues
            var hangfireJobId = request.Operation switch
            {
                ProcessingOperation.Resize => BackgroundJob.Enqueue<IImageProcessingService>(
                    "images",
                    service => service.ResizeImageAsync(new ImageResizeRequest
                    {
                        ImagePath = filePath,
                        OutputPath = outputPath,
                        Width = Convert.ToInt32(request.Parameters.GetValueOrDefault("width", 800)),
                        Height = Convert.ToInt32(request.Parameters.GetValueOrDefault("height", 600)),
                        MaintainAspectRatio = Convert.ToBoolean(request.Parameters.GetValueOrDefault("maintainAspectRatio", true)),
                        JobId = jobId
                    })),
                
                ProcessingOperation.ApplyFilters => BackgroundJob.Enqueue<IImageProcessingService>(
                    "images",
                    service => service.ApplyFiltersAsync(new ImageFilterRequest
                    {
                        ImagePath = filePath,
                        OutputPath = outputPath,
                        Filters = GetFiltersFromParameters(request.Parameters),
                        JobId = jobId
                    })),
                
                ProcessingOperation.GenerateThumbnails => BackgroundJob.Enqueue<IImageProcessingService>(
                    "images",
                    service => service.GenerateThumbnailAsync(new ThumbnailRequest
                    {
                        ImagePath = filePath,
                        OutputPath = outputPath,
                        Size = Convert.ToInt32(request.Parameters.GetValueOrDefault("size", 150)),
                        JobId = jobId
                    })),
                
                _ => BackgroundJob.Enqueue<IImageProcessingService>(
                    "images",
                    service => service.GenerateThumbnailAsync(new ThumbnailRequest
                    {
                        ImagePath = filePath,
                        OutputPath = outputPath,
                        Size = 150,
                        JobId = jobId
                    }))
            };

            _logger.LogInformation("Image processing job scheduled. JobId: {JobId}, HangfireJobId: {HangfireJobId}, Operation: {Operation}", 
                jobId, hangfireJobId, request.Operation);

            return Ok(new ImageProcessingResponse
            {
                JobId = hangfireJobId,
                Status = "Scheduled",
                Message = $"Image processing job ({request.Operation}) has been scheduled",
                StartTime = DateTime.UtcNow,
                OutputPath = outputPath
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading and processing image");
            return StatusCode(500, new { Error = "Error processing image upload", Details = ex.Message });
        }
    }

    /// <summary>
    /// Upload multiple images for batch processing
    /// </summary>
    /// <param name="files">Multiple image files</param>
    /// <param name="operation">Processing operation to apply</param>
    /// <param name="parameters">Processing parameters as JSON string</param>
    /// <returns>Batch processing job information</returns>
    [HttpPost("upload-batch")]
    public async Task<IActionResult> UploadBatch(
        [FromForm] IFormFileCollection files,
        [FromForm] ProcessingOperation operation = ProcessingOperation.GenerateThumbnails,
        [FromForm] string parameters = "{}")
    {
        if (files == null || files.Count == 0)
        {
            return BadRequest(new { Error = "No files uploaded", Code = "NO_FILES" });
        }

        if (files.Count > 10) // Limit batch size
        {
            return BadRequest(new { Error = "Maximum 10 files allowed per batch", Code = "TOO_MANY_FILES" });
        }

        try
        {
            var uploadedFiles = new List<string>();
            var allowedExtensions = _configuration.GetSection("ImageProcessing:AllowedExtensions").Get<string[]>() 
                ?? new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };

            // Upload all files first
            foreach (var file in files)
            {
                if (file.Length == 0) continue;

                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension)) continue;

                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                var uniqueId = Guid.NewGuid().ToString("N")[..8];
                var fileName = $"{timestamp}_{uniqueId}_{Path.GetFileNameWithoutExtension(file.FileName)}{fileExtension}";
                var filePath = Path.Combine(_uploadPath, fileName);
                
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                
                uploadedFiles.Add(filePath);
            }

            if (uploadedFiles.Count == 0)
            {
                return BadRequest(new { Error = "No valid image files found", Code = "NO_VALID_FILES" });
            }

            // Parse parameters
            var paramDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(parameters) 
                ?? new Dictionary<string, object>();

            // Schedule batch processing job
            var batchRequest = new BatchProcessingRequest
            {
                ImagePaths = uploadedFiles,
                OutputDirectory = _processedPath,
                Operation = operation,
                Parameters = paramDict,
                JobId = Guid.NewGuid().ToString()
            };

            var hangfireJobId = BackgroundJob.Enqueue<IImageProcessingService>(
                "images",
                service => service.ProcessBatchAsync(batchRequest));

            _logger.LogInformation("Batch processing job scheduled. JobId: {JobId}, Files: {FileCount}, Operation: {Operation}", 
                batchRequest.JobId, uploadedFiles.Count, operation);

            return Ok(new ImageProcessingResponse
            {
                JobId = hangfireJobId,
                Status = "Scheduled",
                Message = $"Batch processing job ({operation}) scheduled for {uploadedFiles.Count} files",
                StartTime = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in batch upload processing");
            return StatusCode(500, new { Error = "Error processing batch upload", Details = ex.Message });
        }
    }

    /// <summary>
    /// Resize an image (fire-and-forget job)
    /// </summary>
    /// <param name="request">Resize request parameters</param>
    /// <returns>Job information</returns>
    [HttpPost("resize")]
    public IActionResult ResizeImage([FromBody] ImageResizeRequest request)
    {
        try
        {
            request.JobId = Guid.NewGuid().ToString();
            
            var jobId = BackgroundJob.Enqueue<IImageProcessingService>(
                service => service.ResizeImageAsync(request));

            _logger.LogInformation("Image resize job scheduled. JobId: {JobId}", jobId);

            return Ok(new ImageProcessingResponse
            {
                JobId = jobId,
                Status = "Scheduled",
                Message = "Image resize job has been scheduled",
                StartTime = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling resize job");
            return StatusCode(500, "Error scheduling resize job");
        }
    }

    /// <summary>
    /// Apply filters to an image (fire-and-forget job)
    /// </summary>
    /// <param name="request">Filter request parameters</param>
    /// <returns>Job information</returns>
    [HttpPost("apply-filters")]
    public IActionResult ApplyFilters([FromBody] ImageFilterRequest request)
    {
        try
        {
            request.JobId = Guid.NewGuid().ToString();
            
            var jobId = BackgroundJob.Enqueue<IImageProcessingService>(
                service => service.ApplyFiltersAsync(request));

            _logger.LogInformation("Image filter job scheduled. JobId: {JobId}", jobId);

            return Ok(new ImageProcessingResponse
            {
                JobId = jobId,
                Status = "Scheduled",
                Message = "Image filter job has been scheduled",
                StartTime = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling filter job");
            return StatusCode(500, "Error scheduling filter job");
        }
    }

    /// <summary>
    /// Generate thumbnail for an image (fire-and-forget job)
    /// </summary>
    /// <param name="request">Thumbnail request parameters</param>
    /// <returns>Job information</returns>
    [HttpPost("thumbnail")]
    public IActionResult GenerateThumbnail([FromBody] ThumbnailRequest request)
    {
        try
        {
            request.JobId = Guid.NewGuid().ToString();
            
            var jobId = BackgroundJob.Enqueue<IImageProcessingService>(
                service => service.GenerateThumbnailAsync(request));

            _logger.LogInformation("Thumbnail generation job scheduled. JobId: {JobId}", jobId);

            return Ok(new ImageProcessingResponse
            {
                JobId = jobId,
                Status = "Scheduled",
                Message = "Thumbnail generation job has been scheduled",
                StartTime = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling thumbnail job");
            return StatusCode(500, "Error scheduling thumbnail job");
        }
    }

    /// <summary>
    /// Process multiple images in batch (fire-and-forget job)
    /// </summary>
    /// <param name="request">Batch processing request</param>
    /// <returns>Job information</returns>
    [HttpPost("batch")]
    public IActionResult ProcessBatch([FromBody] BatchProcessingRequest request)
    {
        try
        {
            request.JobId = Guid.NewGuid().ToString();
            
            var jobId = BackgroundJob.Enqueue<IImageProcessingService>(
                service => service.ProcessBatchAsync(request));

            _logger.LogInformation("Batch processing job scheduled. JobId: {JobId}", jobId);

            return Ok(new ImageProcessingResponse
            {
                JobId = jobId,
                Status = "Scheduled",
                Message = "Batch processing job has been scheduled",
                StartTime = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling batch job");
            return StatusCode(500, "Error scheduling batch job");
        }
    }

    /// <summary>
    /// Manually trigger cleanup of old images
    /// </summary>
    /// <returns>Job information</returns>
    [HttpPost("cleanup")]
    public IActionResult TriggerCleanup()
    {
        try
        {
            var jobId = BackgroundJob.Enqueue<IImageProcessingService>(
                service => service.CleanupOldImagesAsync());

            _logger.LogInformation("Cleanup job scheduled. JobId: {JobId}", jobId);

            return Ok(new ImageProcessingResponse
            {
                JobId = jobId,
                Status = "Scheduled",
                Message = "Cleanup job has been scheduled",
                StartTime = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling cleanup job");
            return StatusCode(500, "Error scheduling cleanup job");
        }
    }

    /// <summary>
    /// Get job status from Hangfire
    /// </summary>
    /// <param name="jobId">Hangfire job ID</param>
    /// <returns>Job status information</returns>
    [HttpGet("status/{jobId}")]
    public IActionResult GetJobStatus(string jobId)
    {
        try
        {
            var jobData = JobStorage.Current.GetConnection().GetJobData(jobId);
            
            if (jobData == null)
            {
                return NotFound($"Job {jobId} not found");
            }

            return Ok(new
            {
                JobId = jobId,
                State = jobData.State,
                Job = jobData.Job?.ToString(),
                CreatedAt = jobData.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job status for {JobId}", jobId);
            return StatusCode(500, "Error retrieving job status");
        }
    }

    /// <summary>
    /// Schedule a delayed image processing job (delayed job example)
    /// </summary>
    /// <param name="request">Processing request</param>
    /// <param name="delayMinutes">Delay in minutes</param>
    /// <returns>Job information</returns>
    [HttpPost("schedule-delayed")]
    public IActionResult ScheduleDelayedProcessing([FromBody] ImageResizeRequest request, [FromQuery] int delayMinutes = 5)
    {
        try
        {
            request.JobId = Guid.NewGuid().ToString();
            
            var jobId = BackgroundJob.Schedule<IImageProcessingService>(
                service => service.ResizeImageAsync(request),
                TimeSpan.FromMinutes(delayMinutes));

            _logger.LogInformation("Delayed image processing job scheduled. JobId: {JobId}, Delay: {DelayMinutes} minutes", 
                jobId, delayMinutes);

            return Ok(new ImageProcessingResponse
            {
                JobId = jobId,
                Status = "Scheduled",
                Message = $"Image processing job scheduled to run in {delayMinutes} minutes",
                StartTime = DateTime.UtcNow.AddMinutes(delayMinutes)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling delayed job");
            return StatusCode(500, "Error scheduling delayed job");
        }
    }

    /// <summary>
    /// Create a continuation job (job chain example)
    /// </summary>
    /// <param name="request">Initial processing request</param>
    /// <returns>Job information</returns>
    [HttpPost("chain-processing")]
    public IActionResult ChainProcessing([FromBody] ImageResizeRequest request)
    {
        try
        {
            request.JobId = Guid.NewGuid().ToString();
            
            // First job: Resize image
            var resizeJobId = BackgroundJob.Enqueue<IImageProcessingService>(
                "images",
                service => service.ResizeImageAsync(request));

            // Second job: Generate thumbnail after resize completes
            var thumbnailRequest = new ThumbnailRequest
            {
                ImagePath = request.OutputPath,
                OutputPath = request.OutputPath.Replace("processed_", "thumb_"),
                Size = 150,
                JobId = Guid.NewGuid().ToString()
            };

            var thumbnailJobId = BackgroundJob.ContinueJobWith<IImageProcessingService>(
                resizeJobId,
                "images",
                service => service.GenerateThumbnailAsync(thumbnailRequest));

            _logger.LogInformation("Chained processing jobs scheduled. ResizeJobId: {ResizeJobId}, ThumbnailJobId: {ThumbnailJobId}", 
                resizeJobId, thumbnailJobId);

            return Ok(new
            {
                ResizeJobId = resizeJobId,
                ThumbnailJobId = thumbnailJobId,
                Status = "Scheduled",
                Message = "Chained processing jobs scheduled: resize → thumbnail",
                StartTime = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling chained jobs");
            return StatusCode(500, "Error scheduling chained jobs");
        }
    }

    /// <summary>
    /// Demonstrate recurring job management
    /// </summary>
    /// <param name="jobId">Recurring job identifier</param>
    /// <param name="cronExpression">Cron expression for scheduling</param>
    /// <returns>Job information</returns>
    [HttpPost("recurring-job/{jobId}")]
    public IActionResult CreateRecurringJob(string jobId, [FromQuery] string cronExpression = "0 */6 * * *")
    {
        try
        {
            // Create a recurring job for system maintenance
            RecurringJob.AddOrUpdate(
                jobId,
                () => Console.WriteLine($"Recurring maintenance job {jobId} executed at {DateTime.UtcNow}"),
                cronExpression,
                queue: "default");

            _logger.LogInformation("Recurring job created. JobId: {JobId}, Cron: {CronExpression}", jobId, cronExpression);

            return Ok(new
            {
                JobId = jobId,
                CronExpression = cronExpression,
                Status = "Created",
                Message = $"Recurring job '{jobId}' created with schedule: {cronExpression}",
                NextExecution = "Check Hangfire dashboard for next execution"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating recurring job {JobId}", jobId);
            return StatusCode(500, "Error creating recurring job");
        }
    }

    /// <summary>
    /// Remove a recurring job
    /// </summary>
    /// <param name="jobId">Recurring job identifier</param>
    /// <returns>Result</returns>
    [HttpDelete("recurring-job/{jobId}")]
    public IActionResult RemoveRecurringJob(string jobId)
    {
        try
        {
            RecurringJob.RemoveIfExists(jobId);
            
            _logger.LogInformation("Recurring job removed. JobId: {JobId}", jobId);

            return Ok(new
            {
                JobId = jobId,
                Status = "Removed",
                Message = $"Recurring job '{jobId}' has been removed"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing recurring job {JobId}", jobId);
            return StatusCode(500, "Error removing recurring job");
        }
    }

    /// <summary>
    /// Get comprehensive job statistics
    /// </summary>
    /// <returns>Job statistics</returns>
    [HttpGet("job-stats")]
    public IActionResult GetJobStatistics()
    {
        try
        {
            var monitoring = JobStorage.Current.GetMonitoringApi();
            var stats = monitoring.GetStatistics();

            return Ok(new
            {
                Servers = stats.Servers,
                Queues = stats.Queues,
                Jobs = new
                {
                    Enqueued = stats.Enqueued,
                    Failed = stats.Failed,
                    Processing = stats.Processing,
                    Scheduled = stats.Scheduled,
                    Succeeded = stats.Succeeded,
                    Deleted = stats.Deleted,
                    Recurring = stats.Recurring
                },
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job statistics");
            return StatusCode(500, "Error retrieving job statistics");
        }
    }

    /// <summary>
    /// Helper method to extract filters from parameters
    /// </summary>
    private static List<FilterType> GetFiltersFromParameters(Dictionary<string, object> parameters)
    {
        var filters = new List<FilterType>();
        
        if (parameters.ContainsKey("filters"))
        {
            if (parameters["filters"] is string filtersString)
            {
                var filterNames = filtersString.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var filterName in filterNames)
                {
                    if (Enum.TryParse<FilterType>(filterName.Trim(), true, out var filter))
                    {
                        filters.Add(filter);
                    }
                }
            }
        }
        else if (parameters.ContainsKey("filter"))
        {
            if (Enum.TryParse<FilterType>(parameters["filter"].ToString(), true, out var singleFilter))
            {
                filters.Add(singleFilter);
            }
        }
        
        return filters;
    }
}