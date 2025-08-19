using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthDashboard.Api.HealthChecks;

/// <summary>
/// Custom health check for message queue systems (RabbitMQ, Azure Service Bus, etc.)
/// Demonstrates how to check message queue connectivity and performance
/// This is a simulated implementation for educational purposes
/// </summary>
public class MessageQueueHealthCheck : IHealthCheck
{
    private readonly ILogger<MessageQueueHealthCheck> _logger;
    private readonly string _queueName;
    private readonly string _connectionString;
    private readonly int _maxQueueDepth;
    private static readonly Random _random = new();

    public MessageQueueHealthCheck(
        ILogger<MessageQueueHealthCheck> logger, 
        string queueName, 
        string connectionString,
        int maxQueueDepth = 1000)
    {
        _logger = logger;
        _queueName = queueName;
        _connectionString = connectionString;
        _maxQueueDepth = maxQueueDepth;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Checking message queue health: {QueueName}", _queueName);

            // Simulate connection and queue inspection
            var connectionTime = _random.Next(10, 100);
            await Task.Delay(connectionTime, cancellationToken);

            // Simulate queue metrics
            var queueDepth = _random.Next(0, _maxQueueDepth + 200); // Sometimes exceed max
            var messagesPerSecond = _random.Next(0, 100);
            var deadLetterCount = _random.Next(0, 50);
            var consumerCount = _random.Next(1, 10);
            
            var data = new Dictionary<string, object>
            {
                ["queue_name"] = _queueName,
                ["connection_string"] = MaskConnectionString(_connectionString),
                ["connection_time_ms"] = connectionTime,
                ["queue_depth"] = queueDepth,
                ["max_queue_depth"] = _maxQueueDepth,
                ["messages_per_second"] = messagesPerSecond,
                ["dead_letter_count"] = deadLetterCount,
                ["active_consumers"] = consumerCount,
                ["check_timestamp"] = DateTime.UtcNow.ToString("O")
            };

            // Simulate connection failures
            var scenario = _random.Next(1, 101);
            if (scenario <= 5) // 5% chance of connection failure
            {
                _logger.LogError("Message queue connection failed: {QueueName}", _queueName);
                return HealthCheckResult.Unhealthy(
                    $"Unable to connect to message queue '{_queueName}'", 
                    new Exception("Connection refused or authentication failed"), 
                    data);
            }

            // Check queue depth
            if (queueDepth > _maxQueueDepth)
            {
                _logger.LogError("Message queue depth exceeded: {QueueDepth}/{MaxDepth}", queueDepth, _maxQueueDepth);
                return HealthCheckResult.Unhealthy(
                    $"Queue '{_queueName}' depth exceeded maximum: {queueDepth}/{_maxQueueDepth}", 
                    data: data);
            }

            // Check for high queue depth (warning threshold at 80%)
            var warningThreshold = _maxQueueDepth * 0.8;
            if (queueDepth > warningThreshold)
            {
                _logger.LogWarning("Message queue depth is high: {QueueDepth}/{MaxDepth}", queueDepth, _maxQueueDepth);
                return HealthCheckResult.Degraded(
                    $"Queue '{_queueName}' depth is high: {queueDepth}/{_maxQueueDepth}", 
                    data: data);
            }

            // Check for dead letter messages
            if (deadLetterCount > 10)
            {
                _logger.LogWarning("High dead letter count in queue: {DeadLetterCount}", deadLetterCount);
                return HealthCheckResult.Degraded(
                    $"Queue '{_queueName}' has high dead letter count: {deadLetterCount}", 
                    data: data);
            }

            // Check for no active consumers
            if (consumerCount == 0)
            {
                _logger.LogWarning("No active consumers for queue: {QueueName}", _queueName);
                return HealthCheckResult.Degraded(
                    $"Queue '{_queueName}' has no active consumers", 
                    data: data);
            }

            // All checks passed
            _logger.LogInformation("Message queue is healthy: {QueueName}, depth: {QueueDepth}, consumers: {ConsumerCount}", 
                _queueName, queueDepth, consumerCount);
            
            return HealthCheckResult.Healthy(
                $"Queue '{_queueName}' is operating normally. Depth: {queueDepth}, Consumers: {consumerCount}", 
                data);
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("Message queue health check was cancelled: {QueueName}", _queueName);
            return HealthCheckResult.Unhealthy(
                $"Message queue health check was cancelled for '{_queueName}'");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Message queue health check failed: {QueueName}", _queueName);
            return HealthCheckResult.Unhealthy(
                $"Message queue health check failed for '{_queueName}': {ex.Message}", 
                ex);
        }
    }

    private static string MaskConnectionString(string connectionString)
    {
        // Mask sensitive information in connection strings
        return connectionString
            .Replace("Password=", "Password=***;", StringComparison.OrdinalIgnoreCase)
            .Replace("SharedAccessKey=", "SharedAccessKey=***;", StringComparison.OrdinalIgnoreCase)
            .Replace("AccountKey=", "AccountKey=***;", StringComparison.OrdinalIgnoreCase);
    }
}