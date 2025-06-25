namespace OracleConn_BGJobs;

using Hangfire;
using Oracle.ManagedDataAccess.Client;
using System.Data;

public class Worker : BackgroundService {
    private readonly ILogger<Worker> _logger;
    private readonly int delayMilisecondsPerTask = 3000000;  // 5 minutes in milliseconds
    private readonly OracleDataService _oracleDataService;

    public Worker(ILogger<Worker> logger, OracleDataService oracleDataService) {
        _logger = logger;
        _oracleDataService = oracleDataService;
    }

    public void RecuringJob() {
        _logger.LogInformation("[REC] Recuring job executed at: {time}", DateTimeOffset.Now);

        try {
            var result = _oracleDataService.QueryDataAsync(
                "SELECT * FROM VD_CADENA"
            ).GetAwaiter().GetResult();
            foreach (DataRow row in result.Rows) {
                _logger.LogInformation("[REC] Row: {row}", string.Join(", ", row.ItemArray));
            }
        } catch (Exception ex) {
            _logger.LogError(ex, "[REC] Error executing recurring job");
        }

        _logger.LogInformation("[REC] Recurring job completed at: {time}", DateTimeOffset.Now);
    }

    public void EnqueuedJob() {
        _logger.LogInformation("[QUE] Enqueued job executed at: {time}", DateTimeOffset.Now);

        try {
            var result = _oracleDataService.QueryDataAsync(
                "SELECT * FROM VD_CATALOG WHERE CADENA = :cadena",
                new OracleParameter("cadena", 2)
            ).GetAwaiter().GetResult();
            foreach (DataRow row in result.Rows) {
                _logger.LogInformation("[QUE] Row: {row}", string.Join(", ", row.ItemArray));
            }
        } catch (Exception ex) {
            _logger.LogError(ex, "[QUE] Error executing enqueued job");
        }

        _logger.LogInformation("[QUE] Enqueued job completed at: {time}", DateTimeOffset.Now);
        BackgroundJob.Schedule(() => EnqueuedJob(), TimeSpan.FromMinutes(10));// Auto-enqueue the next job every 10 mins
    }

    public override Task StartAsync(CancellationToken cancellationToken) {
        RecurringJob.AddOrUpdate(
            "vitadi_bgjobs",
            () => RecuringJob(),
            "*/10 * * * *");

        BackgroundJob.Enqueue(() => EnqueuedJob());

        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {
            if (_logger.IsEnabled(LogLevel.Information)) {
                _logger.LogInformation("[LIF] Worker running at: {time}", DateTimeOffset.Now);
            }

            //await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            await Task.Delay(delayMilisecondsPerTask, stoppingToken);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken) {
        _logger.LogInformation("[LIF] Worker stopping at: {time}", DateTimeOffset.Now);
        return base.StopAsync(cancellationToken);
    }
}

// Other querys
//// Consulta simple
//var data = await _oracleDataService.QueryDataAsync("SELECT * FROM employees");

//// Consulta con par�metros
//var userData = await _oracleDataService.QueryDataAsync(
//    "SELECT * FROM users WHERE department = :dept AND active = :active",
//    new OracleParameter("dept", "IT"),
//    new OracleParameter("active", 1)
//);
