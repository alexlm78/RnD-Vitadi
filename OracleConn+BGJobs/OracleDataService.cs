namespace OracleConn_BGJobs;

using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using System.Data;

public class OracleDataService {
    private readonly string _connectionString;
    private readonly ILogger<OracleDataService> _logger;

    public OracleDataService(string connectionString, ILogger<OracleDataService> logger) {
        _connectionString = connectionString;
        _logger = logger;
    }

    public async Task<DataTable> QueryDataAsync(string sql, params OracleParameter[] parameters) {
        using var connection = new OracleConnection(_connectionString);
        using var command = new OracleCommand(sql, connection);

        if (parameters != null) {
            command.Parameters.AddRange(parameters);
        }

        var dataTable = new DataTable();

        try {
            await connection.OpenAsync();
            _logger.LogInformation("Executing query: {sql}", sql);

            using var reader = await command.ExecuteReaderAsync();
            dataTable.Load(reader);

            _logger.LogInformation("Query returned {count} rows", dataTable.Rows.Count);
        } catch (Exception ex) {
            _logger.LogError(ex, "Error executing query: {sql}", sql);
            throw;
        }

        return dataTable;
    }
}
