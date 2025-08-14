using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using ImageProcessor.Api;
using ImageProcessor.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Register image processing service
builder.Services.AddScoped<IImageProcessingService, ImageProcessingService>();

// Register authorization filter with dependencies
builder.Services.AddScoped<HangfireAuthorizationFilter>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "ImageProcessor API", 
        Version = "v1",
        Description = "API for background image processing using Hangfire"
    });
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "ImageProcessor.Api.xml"));
});

// Configure Hangfire with SQL Server storage
var hangfireConfig = builder.Configuration.GetSection("Hangfire");
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));

// Add Hangfire server with configuration
var serverConfig = hangfireConfig.GetSection("Server");
builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = serverConfig.GetValue<int>("WorkerCount", 5);
    options.Queues = serverConfig.GetSection("Queues").Get<string[]>() ?? new[] { "default" };
    options.ShutdownTimeout = TimeSpan.Parse(serverConfig.GetValue<string>("ShutdownTimeout") ?? "00:00:30");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ImageProcessor API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

// Configure Hangfire Dashboard with enhanced options
var dashboardConfig = hangfireConfig.GetSection("Dashboard");
IDashboardAuthorizationFilter authFilter = app.Environment.IsDevelopment() 
    ? new HangfireDevAuthorizationFilter()
    : app.Services.GetRequiredService<HangfireAuthorizationFilter>();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { authFilter },
    DashboardTitle = dashboardConfig.GetValue<string>("Title", "Hangfire Dashboard"),
    AppPath = dashboardConfig.GetValue<string>("AppPath", "/"),
    StatsPollingInterval = 2000, // 2 seconds
    DisplayStorageConnectionString = false
});

app.MapControllers();

// Configure recurring jobs with different queues
RecurringJob.AddOrUpdate<IImageProcessingService>(
    "cleanup-old-images",
    service => service.CleanupOldImagesAsync(),
    Cron.Daily,
    queue: "cleanup");

// Example: Add a recurring job for system health monitoring
RecurringJob.AddOrUpdate(
    "system-health-check",
    () => Console.WriteLine($"System health check at {DateTime.UtcNow}"),
    Cron.Hourly,
    queue: "default");

app.Run();
