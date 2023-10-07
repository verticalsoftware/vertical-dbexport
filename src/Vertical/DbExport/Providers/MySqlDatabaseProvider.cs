using Microsoft.Extensions.Logging;
using MySqlConnector;
using Vertical.DbExport.Infrastructure;
using Vertical.DbExport.Options;

namespace Vertical.DbExport.Providers;

[Inject]
public class MySqlDatabaseProvider : IDatabaseProvider
{
    private const int DefaultPort = 3306;
    private readonly ILogger<MySqlDatabaseProvider> _logger;

    public MySqlDatabaseProvider(ILogger<MySqlDatabaseProvider> logger)
    {
        _logger = logger;
    }
    
    public string ProviderId => "mysql";
    
    public async Task<bool> TestConnectionAsync(ConnectionOptions options)
    {
        try
        {
            await using var connection = CreateConnection(options);
            _logger.LogInformation("Testing MySql connection to {server}", connection.DataSource);
            await connection.OpenAsync();
            _logger.LogInformation("Verified connection");
            await connection.CloseAsync();
            return true;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to establish connection to MySql database.");
            return false;
        }
    }

    public TaskBatchOperation CreateBatchingOperation(ConnectionOptions connection, TaskOptions task)
    {
        return new MySqlTaskBatchOperation(connection, task, _logger);
    }

    public static MySqlConnection CreateConnection(ConnectionOptions options)
    {
        var builder = new MySqlConnectionStringBuilder
        {
            Server = options.Server,
            Port = (uint)options.Port.GetValueOrDefault(DefaultPort),
            Database = options.Database,
            UserID = options.UserId,
            Password = options.Password
        };

        return new MySqlConnection(builder.ToString());
    }
}