using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Polly;
using Vertical.DbExport.Options;
using Vertical.DbExport.Services;

namespace Vertical.DbExport.MySql;

[ServiceRegistration(ServiceLifetime.Singleton)]
public class MySqlDatabaseProvider : IDatabaseProvider
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<MySqlDatabaseProvider> _logger;

    /// <summary>
    /// Creates a new instance of this type
    /// </summary>
    public MySqlDatabaseProvider(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<MySqlDatabaseProvider>();
    }
    
    /// <inheritdoc />
    public string DriverId => "MySqlConnector";

    /// <inheritdoc />
    public ConnectionOptions? ConnectionOptions { get; set; }

    /// <inheritdoc />
    public async Task ValidateAsync(CancellationToken cancellationToken)
    {
        await using var connection = await CreateAndOpenConnectionAsync(
            CheckConnectionOptionsSetOrThrow(), 
            cancellationToken);
        
        await connection.QueryAsync<int>("SELECT 1");
        
        _logger.LogDebug("Connection health check to MySql server {server}/{database} complete.", 
            connection.DataSource,
            connection.Database);
    }

    /// <inheritdoc />
    public IDataContext CreateContext(DataSourceOptions dataSource)
    {
        return new MySqlDataContext(
            _loggerFactory.CreateLogger<MySqlDataContext>(),
            ct => CreateAndOpenConnectionAsync(CheckConnectionOptionsSetOrThrow(), ct),
            dataSource);
    }

    private async Task<MySqlConnection> CreateAndOpenConnectionAsync(
        ConnectionOptions options,
        CancellationToken cancellationToken)
    {
        var builder = new MySqlConnectionStringBuilder(options.ConnectionString);
        var connection = new MySqlConnection(builder.ToString());

        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(options.ConnectionRetryCount, attempt =>
            {
                _logger.LogWarning("Connection attempt {attempt} (of {count}) to MySql {server} failed.",
                    attempt,
                    options.ConnectionRetryCount,
                    builder.Server);
                return TimeSpan.FromSeconds(attempt * 5);
            });

        await retryPolicy.ExecuteAsync(() => connection.OpenAsync(cancellationToken));
        _logger.LogTrace("Connected to MySql {server} successfully.", builder.Server);
        return connection;
    }

    private ConnectionOptions CheckConnectionOptionsSetOrThrow()
    {
        return ConnectionOptions ?? throw new InvalidOperationException("Connection options not set.");
    }
}