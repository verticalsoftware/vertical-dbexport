using Microsoft.Extensions.Logging;
using MySqlConnector;
using Vertical.DbExport.Data;
using Vertical.DbExport.Extensions;
using Vertical.DbExport.Models;

namespace Vertical.DbExport.MySql;

public class OffsetDataQuery
{
    private readonly ILogger _logger;
    private readonly ConnectionFactory<MySqlConnection> _connectionFactory;
    private readonly OffsetQueryDefinition _queryDefinition;

    public OffsetDataQuery(
        ILogger logger,
        ConnectionFactory<MySqlConnection> connectionFactory,
        OffsetQueryDefinition queryDefinition)
    {
        _logger = logger;
        _connectionFactory = connectionFactory;
        _queryDefinition = queryDefinition;
    }

    public async Task<IReadOnlyList<Record>> ExecuteAsync(
        long limit,
        long offset,
        CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory(cancellationToken);
        var parameters = new
        {
            limit,
            offset
        };
        
        return await connection.QueryAndLogAsync(
            _queryDefinition.Sql,
            parameters,
            _logger);
    }
}