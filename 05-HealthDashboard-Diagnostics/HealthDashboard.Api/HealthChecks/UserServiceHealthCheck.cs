using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthDashboard.Api.HealthChecks;

/// <summary>
/// Health check for User Service
/// </summary>
public class UserServiceHealthCheck : IHealthCheck
{
    private readonly CustomServiceHealthCheck _customServiceHealthCheck;

    public UserServiceHealthCheck(ILogger<UserServiceHealthCheck> logger)
    {
        _customServiceHealthCheck = new CustomServiceHealthCheck(
            logger as ILogger<CustomServiceHealthCheck> ?? 
            new Microsoft.Extensions.Logging.Abstractions.NullLogger<CustomServiceHealthCheck>(), 
            "User Service");
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return _customServiceHealthCheck.CheckHealthAsync(context, cancellationToken);
    }
}