using System.Data;
using System.Text;
using Dapper;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Polly;
using Vertical.DbExport.Options;
using Vertical.DbExport.Utilities;

namespace Vertical.DbExport.Providers;

public class MySqlTaskBatchOperation : TaskBatchOperation
{
    private readonly ConnectionOptions _connectionOptions;
    private readonly TaskOptions _taskOptions;
    private readonly ILogger _logger;
    private string? _queryClause;
    private MySqlConnection? _connection;
    private int _offset;
    
    public MySqlTaskBatchOperation(ConnectionOptions connectionOptions, TaskOptions taskOptions, ILogger logger)
    {
        _connectionOptions = connectionOptions;
        _taskOptions = taskOptions;
        _logger = logger;
    }
    
    public override async Task<IEnumerable<object>> QueryAsync(CancellationToken cancellationToken)
    {
        var connection = await GetConnectionAsync();
        var command = await BuildQueryStringAsync(connection);
        
        Ready = false;
        return Enumerable.Empty<object>();
    }

    public override async ValueTask DisposeAsync()
    {
        if (_connection == null)
            return;

        await _connection.CloseAsync();
        await _connection.DisposeAsync();
        _connection = null;
    }

    private async Task<string> BuildQueryStringAsync(MySqlConnection connection)
    {
        if (_queryClause != null)
            return _queryClause;

        if (!string.IsNullOrWhiteSpace(_taskOptions.Query))
            return _queryClause = _taskOptions.Query;

        var sqlBuilder = new StringBuilder();
        sqlBuilder.AppendLine("select src.*");
        sqlBuilder.AppendLine("from (");
        sqlBuilder.Append("  select ");

        var columns = _taskOptions.Select is { Length: > 0 }
            ? _taskOptions.Select
            : await GetColumnNamesAsync(connection);

        sqlBuilder.AppendCsvLine(columns).AppendLine();
        sqlBuilder.Append("  from ")
            .Append(_taskOptions.Schema).Append('.')
            .AppendLine(_taskOptions.Table);

        if (_taskOptions.Sort is { Length: > 0 })
            sqlBuilder.Append("  order by ").AppendCsvLine(_taskOptions.Sort);
        
        sqlBuilder.AppendLine(") src");

        if (_taskOptions.BatchCount.HasValue)
        {
            sqlBuilder.AppendLine("limit @offset, @count");
        }

        _queryClause = sqlBuilder.ToString();
        _logger.LogDebug("Built the following SQL query: {query}", _queryClause);
        
        return _queryClause;
    }

    private async Task<string[]> GetColumnNamesAsync(MySqlConnection connection)
    {
        _logger.LogDebug("Retrieving column names from information_schema");
        
        var sql = "select column_name " +
                  "from information_schema " +
                  "order by ordinal_position" +
                  "where table_schema=@tableSchema AND table_name=@tableName";

        var results = await connection.QueryAsync<string>(sql, param: new
        {
            tableSchema = _taskOptions.Schema,
            tableName = _taskOptions.Table
        });

        return results.ToArray();
    }

    private async Task<MySqlConnection> GetConnectionAsync()
    {
        if (_connection is { State: ConnectionState.Open })
            return _connection;

        if (_connection != null)
        {
            _logger.LogWarning("MySqlConnection '{server}:{database}' is broken, reconnecting", 
                _connection.DataSource, 
                _connection.Database);
            
            var brokeConnection = _connection;
            _connection = null;
            await brokeConnection.CloseAsync();
            await brokeConnection.DisposeAsync();
        }

        var policy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: _connectionOptions.ConnectRetries.GetValueOrDefault(3),
                sleepDurationProvider: count => TimeSpan.FromSeconds(count * 3));
        
        var newConnection = MySqlDatabaseProvider.CreateConnection(_connectionOptions);
        _logger.LogDebug("Connecting to mySql '{server}:{database}'", newConnection.DataSource, newConnection.Database);
        await policy.ExecuteAsync(() => newConnection.OpenAsync());

        _logger.LogDebug("Connection mySql '{server}:{database}' established", newConnection.DataSource, newConnection.Database);
        return _connection = newConnection;
    }
}