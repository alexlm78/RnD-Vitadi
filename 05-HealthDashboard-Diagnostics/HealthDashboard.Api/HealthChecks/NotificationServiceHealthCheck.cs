using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthDashboard.Api.HealthChecks;

/// <summary>
/// Health check for Notification Service
/// </summary>
public class NotificationServiceHealthCheck : IHealthCheck
{
    private readonly CustomServiceHealthCheck _customServiceHealthCheck;

    public NotificationServiceHealthCheck(ILogger<NotificationServiceHealthCheck> logger)
    {
        _customServiceHealthCheck = new CustomServiceHealthCheck(
            logger as ILogger<CustomServiceHealthCheck> ?? 
            new Microsoft.Extensions.Logging.Abstractions.NullLogger<CustomServiceHealthCheck>(), 
            "Notification Service");
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return _customServiceHealthCheck.CheckHealthAsync(context, cancellationToken);
    }
}