using Hangfire.Dashboard;
using System.Text;

namespace ImageProcessor.Api;

/// <summary>
/// Authorization filter for Hangfire Dashboard with basic authentication support.
/// Provides both development and production-ready authentication options.
/// </summary>
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public HangfireAuthorizationFilter(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // In development, allow access from localhost without authentication
        if (_environment.IsDevelopment())
        {
            var remoteIp = httpContext.Connection.RemoteIpAddress?.ToString();
            if (remoteIp == "127.0.0.1" || remoteIp == "::1" || remoteIp == null)
            {
                return true;
            }
        }

        // Check for basic authentication
        return CheckBasicAuthentication(httpContext);
    }

    private bool CheckBasicAuthentication(HttpContext httpContext)
    {
        var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
        
        if (authHeader != null && authHeader.StartsWith("Basic "))
        {
            var encodedCredentials = authHeader.Substring("Basic ".Length).Trim();
            var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
            var parts = credentials.Split(':', 2);
            
            if (parts.Length == 2)
            {
                var username = parts[0];
                var password = parts[1];
                
                // Get credentials from configuration
                var configUsername = _configuration["Hangfire:Dashboard:Username"] ?? "admin";
                var configPassword = _configuration["Hangfire:Dashboard:Password"] ?? "password123";
                
                return username == configUsername && password == configPassword;
            }
        }

        // Send WWW-Authenticate header to prompt for credentials
        httpContext.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Hangfire Dashboard\"";
        httpContext.Response.StatusCode = 401;
        
        return false;
    }
}

/// <summary>
/// Simple authorization filter for development environments.
/// Allows unrestricted access for easier development and testing.
/// </summary>
public class HangfireDevAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        // Allow all access in development
        return true;
    }
}