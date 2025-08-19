using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthDashboard.Api.HealthChecks;

/// <summary>
/// Health check for Payment Service
/// </summary>
public class PaymentServiceHealthCheck : IHealthCheck
{
    private readonly CustomServiceHealthCheck _customServiceHealthCheck;

    public PaymentServiceHealthCheck(ILogger<PaymentServiceHealthCheck> logger)
    {
        _customServiceHealthCheck = new CustomServiceHealthCheck(
            logger as ILogger<CustomServiceHealthCheck> ?? 
            new Microsoft.Extensions.Logging.Abstractions.NullLogger<CustomServiceHealthCheck>(), 
            "Payment Service");
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        return _customServiceHealthCheck.CheckHealthAsync(context, cancellationToken);
    }
}