using HealthDashboard.Api.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace HealthDashboard.Api.Examples;

/// <summary>
/// Examples of how to test custom health checks
/// These examples show different testing patterns for health check implementations
/// </summary>
public class HealthCheckTestingExamples
{
    /// <summary>
    /// Example of testing a health check with mocked dependencies
    /// This would typically be in a test project using xUnit, NUnit, or MSTest
    /// </summary>
    public static async Task<bool> TestMemoryHealthCheckExample()
    {
        // Arrange
        var logger = new TestLogger<MemoryHealthCheck>();
        var maxMemoryBytes = 1024 * 1024 * 100; // 100MB limit
        var healthCheck = new MemoryHealthCheck(logger, maxMemoryBytes);
        var context = new HealthCheckContext();

        try
        {
            // Act
            var result = await healthCheck.CheckHealthAsync(context);

            // Assert
            Console.WriteLine($"Health Check Result: {result.Status}");
            Console.WriteLine($"Description: {result.Description}");
            
            if (result.Data != null)
            {
                foreach (var data in result.Data)
                {
                    Console.WriteLine($"  {data.Key}: {data.Value}");
                }
            }

            return result.Status != HealthStatus.Unhealthy;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Health check failed with exception: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Example of testing health checks with different scenarios
    /// </summary>
    public static async Task TestHealthCheckScenarios()
    {
        var logger = new TestLogger<CustomServiceHealthCheck>();
        var healthCheck = new CustomServiceHealthCheck(logger, "TestService");

        // Test multiple times to see different random scenarios
        for (int i = 0; i < 10; i++)
        {
            var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());
            Console.WriteLine($"Test {i + 1}: {result.Status} - {result.Description}");
            
            // Small delay to see different random results
            await Task.Delay(100);
        }
    }

    /// <summary>
    /// Example of testing health check timeout behavior
    /// </summary>
    public static async Task TestHealthCheckTimeout()
    {
        var logger = new TestLogger<ExternalApiHealthCheck>();
        var httpClient = new HttpClient();
        
        // Use a very short timeout to test timeout behavior
        var healthCheck = new ExternalApiHealthCheck(
            httpClient, 
            logger, 
            "https://httpstat.us/200?sleep=5000", // This will take 5 seconds
            TimeSpan.FromSeconds(1)); // But we only wait 1 second

        try
        {
            var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());
            Console.WriteLine($"Timeout Test Result: {result.Status} - {result.Description}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Timeout test exception: {ex.Message}");
        }
    }

    /// <summary>
    /// Example of testing health check data collection
    /// </summary>
    public static async Task TestHealthCheckDataCollection()
    {
        var logger = new TestLogger<SqlConnectionHealthCheck>();
        var healthCheck = new SqlConnectionHealthCheck(
            logger, 
            "Server=localhost;Database=TestDb;Trusted_Connection=true;",
            "SELECT 1");

        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());
        
        Console.WriteLine($"Data Collection Test: {result.Status}");
        Console.WriteLine("Collected Data:");
        
        if (result.Data != null)
        {
            foreach (var kvp in result.Data)
            {
                Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
            }
        }
    }

    /// <summary>
    /// Example of testing health check with cancellation
    /// </summary>
    public static async Task TestHealthCheckCancellation()
    {
        var logger = new TestLogger<MessageQueueHealthCheck>();
        var healthCheck = new MessageQueueHealthCheck(
            logger, 
            "test-queue", 
            "amqp://localhost:5672");

        using var cts = new CancellationTokenSource();
        
        // Cancel after 100ms
        cts.CancelAfter(100);

        try
        {
            var result = await healthCheck.CheckHealthAsync(new HealthCheckContext(), cts.Token);
            Console.WriteLine($"Cancellation Test Result: {result.Status}");
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Health check was successfully cancelled");
        }
    }
}

/// <summary>
/// Simple test logger implementation for testing health checks
/// In a real test project, you would use a proper mocking framework like Moq
/// </summary>
public class TestLogger<T> : ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        Console.WriteLine($"[{logLevel}] {typeof(T).Name}: {message}");
        
        if (exception != null)
        {
            Console.WriteLine($"Exception: {exception}");
        }
    }
}

/// <summary>
/// Example of how to run health check tests
/// This would typically be called from a test runner or console application
/// </summary>
public static class HealthCheckTestRunner
{
    public static async Task RunAllTests()
    {
        Console.WriteLine("=== Health Check Testing Examples ===\n");

        Console.WriteLine("1. Testing Memory Health Check:");
        var memoryTestResult = await HealthCheckTestingExamples.TestMemoryHealthCheckExample();
        Console.WriteLine($"Memory test passed: {memoryTestResult}\n");

        Console.WriteLine("2. Testing Health Check Scenarios:");
        await HealthCheckTestingExamples.TestHealthCheckScenarios();
        Console.WriteLine();

        Console.WriteLine("3. Testing Health Check Timeout:");
        await HealthCheckTestingExamples.TestHealthCheckTimeout();
        Console.WriteLine();

        Console.WriteLine("4. Testing Health Check Data Collection:");
        await HealthCheckTestingExamples.TestHealthCheckDataCollection();
        Console.WriteLine();

        Console.WriteLine("5. Testing Health Check Cancellation:");
        await HealthCheckTestingExamples.TestHealthCheckCancellation();
        Console.WriteLine();

        Console.WriteLine("=== All tests completed ===");
    }
}