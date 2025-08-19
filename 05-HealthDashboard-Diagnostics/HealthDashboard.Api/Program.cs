using HealthDashboard.Api.Data;
using HealthDashboard.Api.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure Entity Framework with In-Memory database for demonstration
builder.Services.AddDbContext<HealthDashboardDbContext>(options =>
    options.UseInMemoryDatabase("HealthDashboardDb"));

// Configure HttpClient for external API health checks
builder.Services.AddHttpClient();

// Configure Health Checks with custom implementations
builder.Services.AddHealthChecks()
    // Database health check
    .AddDbContextCheck<HealthDashboardDbContext>("database", 
        tags: new[] { "db", "ready" })
    
    // Self health check
    .AddCheck("self", () => HealthCheckResult.Healthy("API is running"), 
        tags: new[] { "api", "ready" })
    
    // External API health check (using a public API for demonstration)
    .AddCheck<ExternalApiHealthCheck>("external-api-jsonplaceholder", tags: new[] { "external", "api" })
    
    // File system health check
    .AddCheck<FileSystemHealthCheck>("file-system", tags: new[] { "filesystem", "storage" })
    
    // Memory health check
    .AddCheck<MemoryHealthCheck>("memory", tags: new[] { "memory", "performance" })
    
    // Custom service health checks (simulated services)
    .AddCheck<PaymentServiceHealthCheck>("payment-service", tags: new[] { "service", "business" })
    .AddCheck<NotificationServiceHealthCheck>("notification-service", tags: new[] { "service", "business" })
    .AddCheck<UserServiceHealthCheck>("user-service", tags: new[] { "service", "business" });

// Register health check dependencies
builder.Services.AddSingleton<ExternalApiHealthCheck>(provider =>
    new ExternalApiHealthCheck(
        provider.GetRequiredService<HttpClient>(),
        provider.GetRequiredService<ILogger<ExternalApiHealthCheck>>(),
        "https://jsonplaceholder.typicode.com/posts/1",
        TimeSpan.FromSeconds(10)));

builder.Services.AddSingleton<FileSystemHealthCheck>(provider =>
    new FileSystemHealthCheck(
        provider.GetRequiredService<ILogger<FileSystemHealthCheck>>(),
        Path.GetTempPath(),
        1024 * 1024 * 50)); // 50MB minimum free space

builder.Services.AddSingleton<MemoryHealthCheck>(provider =>
    new MemoryHealthCheck(
        provider.GetRequiredService<ILogger<MemoryHealthCheck>>(),
        1024 * 1024 * 512)); // 512MB maximum memory usage

var app = builder.Build();

// Ensure database is created (for In-Memory database)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<HealthDashboardDbContext>();
    context.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.MapControllers();

// Configure detailed health check endpoints with custom response formatting
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            Status = report.Status.ToString(),
            TotalDuration = report.TotalDuration.TotalMilliseconds,
            Checks = report.Entries.Select(entry => new
            {
                Name = entry.Key,
                Status = entry.Value.Status.ToString(),
                Duration = entry.Value.Duration.TotalMilliseconds,
                Description = entry.Value.Description,
                Tags = entry.Value.Tags,
                Data = entry.Value.Data,
                Exception = entry.Value.Exception?.Message
            })
        };
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            WriteIndented = true
        }));
    }
});

// Readiness probe - checks if the application is ready to serve requests
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            Status = report.Status.ToString(),
            Checks = report.Entries.Where(e => e.Value.Tags.Contains("ready")).Select(entry => new
            {
                Name = entry.Key,
                Status = entry.Value.Status.ToString(),
                Description = entry.Value.Description
            })
        };
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
});

// Liveness probe - checks if the application is alive
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("api"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            Status = report.Status.ToString(),
            Timestamp = DateTime.UtcNow
        };
        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));
    }
});

// Health checks by category
app.MapHealthChecks("/health/database", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("db")
});

app.MapHealthChecks("/health/external", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("external")
});

app.MapHealthChecks("/health/services", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("service")
});

// Serve the HTML dashboard
app.MapGet("/dashboard", async context =>
{
    context.Response.ContentType = "text/html";
    var htmlPath = Path.Combine(app.Environment.ContentRootPath, "Views", "HealthDashboard.html");
    if (File.Exists(htmlPath))
    {
        await context.Response.SendFileAsync(htmlPath);
    }
    else
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync("Dashboard not found");
    }
});

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
