# Health Dashboard - Diagnostics

This mini-application demonstrates advanced health check implementations in .NET Core 8, including custom health checks for external services, file systems, memory usage, and business services, along with a visual dashboard for monitoring.

## 🎯 Learning Objectives

- Implement custom health checks for various system components
- Create health check endpoints with detailed information
- Build a visual dashboard for health monitoring
- Understand health check categorization and filtering
- Learn about health check data collection and reporting

## 🏗️ Architecture

```
HealthDashboard.Api/
├── Controllers/
│   └── HealthController.cs          # API endpoints for health data
├── Data/
│   └── HealthDashboardDbContext.cs  # Simple DbContext for DB health checks
├── HealthChecks/                    # Custom health check implementations
│   ├── ExternalApiHealthCheck.cs   # External API dependency checks
│   ├── FileSystemHealthCheck.cs    # File system and disk space checks
│   ├── MemoryHealthCheck.cs        # Memory usage monitoring
│   ├── CustomServiceHealthCheck.cs # Base for business service checks
│   ├── PaymentServiceHealthCheck.cs    # Payment service simulation
│   ├── NotificationServiceHealthCheck.cs # Notification service simulation
│   └── UserServiceHealthCheck.cs   # User service simulation
├── Views/
│   └── HealthDashboard.html         # Visual dashboard UI
└── Program.cs                       # Health check configuration
```

## 🔧 Features Implemented

### 1. Custom Health Checks

#### External API Health Check
- Tests connectivity to external APIs
- Configurable timeout and URL
- Different status levels based on response codes
- Detailed response time tracking

#### File System Health Check
- Verifies directory existence and write permissions
- Monitors available disk space
- Configurable minimum free space thresholds
- Detailed disk usage reporting

#### Memory Health Check
- Monitors application memory usage
- Tracks working set, private memory, and GC metrics
- Configurable memory limits with warning thresholds
- Detailed memory statistics

#### Business Service Health Checks
- Simulated health checks for business services
- Random latency and availability simulation
- Demonstrates different health states (Healthy, Degraded, Unhealthy)
- Realistic business service monitoring patterns

### 2. Health Check Endpoints

#### Basic Endpoints
- `/health` - Complete health status with detailed JSON response
- `/health/ready` - Readiness probe for container orchestration
- `/health/live` - Liveness probe for basic availability

#### Category-Based Endpoints
- `/health/database` - Database-specific health checks
- `/health/external` - External dependency health checks
- `/health/services` - Business service health checks

#### API Controller Endpoints
- `/api/health/detailed` - Comprehensive health information via controller
- `/api/health/status` - Summary health statistics
- `/api/health/category/{tag}` - Health checks filtered by category
- `/api/health/categories` - Available health check categories

### 3. Visual Dashboard

#### Interactive Web UI
- Real-time health status visualization
- Color-coded health cards (Green/Yellow/Red)
- Detailed health check information
- Auto-refresh every 30 seconds
- Responsive design for mobile and desktop

#### Dashboard Features
- System overview with statistics
- Individual health check cards with details
- Error information display
- Last updated timestamp
- Manual refresh capability

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK
- Any code editor (Visual Studio, VS Code, etc.)

### Running the Application

1. **Navigate to the project directory:**
   ```bash
   cd 05-HealthDashboard-Diagnostics/HealthDashboard.Api
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Run the application:**
   ```bash
   dotnet run
   ```

4. **Access the endpoints:**
   - Swagger UI: `https://localhost:5001/swagger`
   - Health Dashboard: `https://localhost:5001/dashboard`
   - Basic Health Check: `https://localhost:5001/health`
   - Detailed API: `https://localhost:5001/api/health/detailed`

## 📊 Health Check Categories

The application organizes health checks into logical categories using tags:

| Category | Description | Health Checks |
|----------|-------------|---------------|
| `api` | Core API functionality | self, external-api |
| `db` | Database connectivity | database |
| `external` | External dependencies | external-api-jsonplaceholder |
| `filesystem` | File system operations | file-system |
| `memory` | Memory usage monitoring | memory |
| `performance` | Performance-related checks | memory |
| `ready` | Readiness indicators | database, self |
| `service` | Business services | payment-service, notification-service, user-service |
| `storage` | Storage systems | file-system |
| `business` | Business logic services | payment-service, notification-service, user-service |

## 🔍 Testing Health Checks

### Using cURL

1. **Basic health check:**
   ```bash
   curl https://localhost:5001/health
   ```

2. **Detailed health information:**
   ```bash
   curl https://localhost:5001/api/health/detailed
   ```

3. **Service category health:**
   ```bash
   curl https://localhost:5001/api/health/category/service
   ```

4. **Available categories:**
   ```bash
   curl https://localhost:5001/api/health/categories
   ```

### Using the Dashboard

1. Open `https://localhost:5001/dashboard` in your browser
2. View real-time health status of all services
3. Click refresh to manually update status
4. Observe different health states as services simulate various conditions

## 🎨 Health Check States

The application demonstrates three health states:

### ✅ Healthy
- All systems operating normally
- Response times within acceptable limits
- No errors or issues detected

### ⚠️ Degraded
- System is functional but experiencing issues
- High latency or performance problems
- Non-critical errors that don't prevent operation

### ❌ Unhealthy
- System is not functioning properly
- Critical errors or failures
- Service unavailable or unresponsive

## 🔧 Configuration

### Health Check Settings

Health checks can be configured in `Program.cs`. See `Examples/CustomHealthCheckRegistration.cs` for comprehensive examples:

```csharp
// External API timeout
new ExternalApiHealthCheck(httpClient, logger, "https://api.example.com", TimeSpan.FromSeconds(10))

// File system minimum free space (50MB)
new FileSystemHealthCheck(logger, "/path/to/monitor", 1024 * 1024 * 50)

// Memory usage limit (512MB)
new MemoryHealthCheck(logger, 1024 * 1024 * 512)

// Redis cache with timeout
new RedisHealthCheck(logger, "localhost:6379", TimeSpan.FromSeconds(5))

// SQL Server with custom query
new SqlConnectionHealthCheck(logger, connectionString, "SELECT 1", TimeSpan.FromSeconds(30))

// Message queue with depth monitoring
new MessageQueueHealthCheck(logger, "orders-queue", connectionString, maxQueueDepth: 1000)
```

### Configuration from appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MyApp;Trusted_Connection=true;",
    "Redis": "localhost:6379",
    "MessageQueue": "amqp://guest:guest@localhost:5672/"
  },
  "HealthChecks": {
    "Redis": {
      "Timeout": "00:00:05",
      "Enabled": true
    },
    "SqlServer": {
      "TestQuery": "SELECT COUNT(*) FROM Users",
      "Timeout": "00:00:30",
      "Enabled": true
    }
  }
}
```

### Custom Health Check Tags

Tags are used to categorize and filter health checks:

```csharp
.AddCheck<CustomHealthCheck>("my-check", tags: new[] { "custom", "business" })
```

## 📈 Monitoring Integration

This implementation is designed to integrate with monitoring systems:

### Container Orchestration
- `/health/ready` - Kubernetes readiness probe
- `/health/live` - Kubernetes liveness probe

### Monitoring Tools
- Prometheus metrics can be added to health checks
- JSON endpoints for integration with monitoring dashboards
- Structured logging for centralized log analysis

### Application Performance Monitoring (APM)
- Health check data includes timing information
- Exception details for troubleshooting
- Custom data fields for business metrics

## 🧪 Simulated Scenarios

The business service health checks simulate realistic scenarios:

1. **Random Latency:** Services exhibit varying response times
2. **Intermittent Failures:** Occasional unhealthy states
3. **Performance Degradation:** High latency conditions
4. **Recovery Patterns:** Services that recover over time

## 🔧 Additional Custom Health Check Examples

The project includes several additional custom health check examples to demonstrate different patterns:

### Redis Health Check (`RedisHealthCheck.cs`)
- **Purpose:** Demonstrates cache service monitoring
- **Features:**
  - Connection timeout handling
  - Latency monitoring with thresholds
  - Sensitive data masking in connection strings
  - Simulated cache metrics (hit ratio, memory usage)
- **Usage Example:**
  ```csharp
  services.AddHealthChecks()
      .AddCheck<RedisHealthCheck>("redis-cache", 
          tags: new[] { "cache", "external" });
  ```

### SQL Connection Health Check (`SqlConnectionHealthCheck.cs`)
- **Purpose:** Demonstrates database connectivity monitoring
- **Features:**
  - Custom test query execution
  - Connection and query timing
  - Database performance metrics
  - Connection string security (password masking)
  - Timeout and error handling
- **Usage Example:**
  ```csharp
  services.AddHealthChecks()
      .AddCheck<SqlConnectionHealthCheck>("sql-database", 
          tags: new[] { "database", "critical" });
  ```

### Message Queue Health Check (`MessageQueueHealthCheck.cs`)
- **Purpose:** Demonstrates message queue system monitoring
- **Features:**
  - Queue depth monitoring with thresholds
  - Dead letter queue tracking
  - Consumer count verification
  - Message throughput metrics
  - Connection security
- **Usage Example:**
  ```csharp
  services.AddHealthChecks()
      .AddCheck<MessageQueueHealthCheck>("order-queue", 
          tags: new[] { "messaging", "business" });
  ```

### Key Patterns Demonstrated

1. **Threshold-Based Health States:**
   - Healthy: Normal operation within limits
   - Degraded: Warning thresholds exceeded but still functional
   - Unhealthy: Critical thresholds exceeded or service unavailable

2. **Security Best Practices:**
   - Connection string masking for sensitive data
   - No logging of passwords or keys
   - Safe error message handling

3. **Performance Monitoring:**
   - Response time tracking
   - Resource utilization metrics
   - Throughput and capacity monitoring

4. **Realistic Simulation:**
   - Random scenarios for testing
   - Different failure modes
   - Recovery patterns and intermittent issues

## 📚 Key Concepts Demonstrated

### 1. IHealthCheck Interface
- Custom implementation of health check logic
- Async health check execution
- Rich health check results with data

### 2. Health Check Registration
- Service registration in dependency injection
- Tag-based categorization
- Configuration of health check options

### 3. Health Check Middleware
- Built-in ASP.NET Core health check middleware
- Custom response formatting
- Conditional health check execution

### 4. Health Check Data
- Custom data collection in health checks
- Structured health check responses
- Performance metrics and diagnostics

## 🔍 Troubleshooting

### Common Issues

1. **External API timeouts:**
   - Check network connectivity
   - Verify API endpoint availability
   - Adjust timeout settings

2. **File system permissions:**
   - Ensure write permissions to monitored directories
   - Check disk space availability
   - Verify directory paths exist

3. **Memory thresholds:**
   - Adjust memory limits based on environment
   - Monitor GC behavior and memory patterns
   - Consider memory leak detection

### Debugging Health Checks

1. **Enable detailed logging:**
   ```json
   {
     "Logging": {
       "LogLevel": {
         "Microsoft.Extensions.Diagnostics.HealthChecks": "Debug"
       }
     }
   }
   ```

2. **Check individual health checks:**
   ```bash
   curl https://localhost:5001/api/health/category/external
   ```

3. **Monitor dashboard for real-time status:**
   - Use browser developer tools to inspect API calls
   - Check console for JavaScript errors
   - Verify network requests to health endpoints

4. **Test health checks programmatically:**
   - See `Examples/HealthCheckTestingExamples.cs` for testing patterns
   - Use the provided test runner to validate health check behavior
   - Test different scenarios including timeouts and cancellation

## 🛠️ Creating Custom Health Checks

### Step-by-Step Guide

1. **Implement IHealthCheck Interface:**
   ```csharp
   public class MyCustomHealthCheck : IHealthCheck
   {
       private readonly ILogger<MyCustomHealthCheck> _logger;
       
       public MyCustomHealthCheck(ILogger<MyCustomHealthCheck> logger)
       {
           _logger = logger;
       }
       
       public async Task<HealthCheckResult> CheckHealthAsync(
           HealthCheckContext context, 
           CancellationToken cancellationToken = default)
       {
           try
           {
               // Your health check logic here
               var isHealthy = await CheckServiceAsync();
               
               var data = new Dictionary<string, object>
               {
                   ["check_time"] = DateTime.UtcNow,
                   ["service_version"] = "1.0.0"
               };
               
               return isHealthy 
                   ? HealthCheckResult.Healthy("Service is running", data)
                   : HealthCheckResult.Unhealthy("Service is down", data: data);
           }
           catch (Exception ex)
           {
               return HealthCheckResult.Unhealthy($"Health check failed: {ex.Message}", ex);
           }
       }
   }
   ```

2. **Register in Program.cs:**
   ```csharp
   builder.Services.AddHealthChecks()
       .AddCheck<MyCustomHealthCheck>("my-service", 
           tags: new[] { "custom", "business" });
   ```

3. **Add Dependencies (if needed):**
   ```csharp
   builder.Services.AddSingleton<MyCustomHealthCheck>(provider =>
       new MyCustomHealthCheck(
           provider.GetRequiredService<ILogger<MyCustomHealthCheck>>(),
           "custom-parameter"));
   ```

### Health Check Best Practices

1. **Use Appropriate Return Types:**
   - `Healthy`: Service is fully operational
   - `Degraded`: Service has issues but is still functional
   - `Unhealthy`: Service is not working properly

2. **Include Useful Data:**
   - Response times
   - Resource usage
   - Configuration values (masked if sensitive)
   - Timestamps

3. **Handle Exceptions Gracefully:**
   - Always wrap in try-catch
   - Log errors appropriately
   - Return meaningful error messages

4. **Use Cancellation Tokens:**
   - Respect cancellation requests
   - Set appropriate timeouts
   - Handle OperationCanceledException

5. **Tag Your Health Checks:**
   - Use tags for categorization
   - Enable filtering by category
   - Support different probe types (readiness, liveness)

## 🎓 Learning Exercises

1. **Create a custom health check** for a specific database table
2. **Add authentication** to the health dashboard
3. **Implement email notifications** for unhealthy services
4. **Create health check metrics** for Prometheus
5. **Add historical health data** storage and visualization
6. **Build a health check** that monitors disk space on multiple drives
7. **Create a composite health check** that combines multiple service checks
8. **Implement a health check** with configurable thresholds from appsettings.json

## 📖 Additional Resources

- [ASP.NET Core Health Checks Documentation](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks)
- [Health Check Middleware](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks#health-check-middleware)
- [Kubernetes Health Checks](https://kubernetes.io/docs/tasks/configure-pod-container/configure-liveness-readiness-startup-probes/)
- [Monitoring and Diagnostics](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/monitor-app-health)

This implementation provides a comprehensive foundation for understanding health checks in .NET applications and can be extended for production use cases.