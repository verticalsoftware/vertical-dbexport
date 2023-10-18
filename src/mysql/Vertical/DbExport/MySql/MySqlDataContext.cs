using Microsoft.Extensions.Logging;
using MySqlConnector;
using Vertical.DbExport.Data;
using Vertical.DbExport.Models;
using Vertical.DbExport.Options;
using Vertical.DbExport.Services;

namespace Vertical.DbExport.MySql;

internal sealed class MySqlDataContext : IDataContext
{
    private readonly ILogger _logger;
    private readonly ConnectionFactory<MySqlConnection> _connectionFactory;
    private readonly DataSourceOptions _dataSource;

    internal MySqlDataContext(
        ILogger logger,
        ConnectionFactory<MySqlConnection> connectionFactory,
        DataSourceOptions dataSource)
    {
        _logger = logger;
        _connectionFactory = connectionFactory;
        _dataSource = dataSource;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ColumnSchema>> GetColumnSchemaAsync(CancellationToken cancellationToken)
    {
        var query = new ColumnSchemaQuery(_logger, _connectionFactory, _dataSource.TableName);
        var columnSchema = await query.ExecuteAsync(cancellationToken);
        
        var includeSet = new HashSet<string>(_dataSource.SelectColumns is { Length: not 0 }
                ? _dataSource.SelectColumns
                : columnSchema.Select(column => column.ColumnName),
            StringComparer.OrdinalIgnoreCase);

        var excludeSet = new HashSet<string>(_dataSource.ExcludeColumns ?? Array.Empty<string>());

        return columnSchema
            .Where(column =>
            {
                var select = includeSet.Contains(column.ColumnName) && !excludeSet.Contains(column.ColumnName);
                _logger.LogDebug("Select schema column {columnName} ({type}, {nullable})={result}", 
                    column.ColumnName,
                    column.DataType.Name,
                    column.IsNullable ? "nullable" : "not nullable",
                    select);
                return select;
            })
            .ToArray();
    }

    /// <inheritdoc />
    public Task<OffsetQueryDefinition> CreateOffsetQueryDefinition(
        IReadOnlyList<ColumnSchema> columnSchemata,
        CancellationToken cancellationToken)
    {
        var queryParameters = _dataSource.SortColumn != null
            ? CreateSortedQueryParameters(
                columnSchemata, 
                columnSchemata.First(c => c.ColumnName == _dataSource.SortColumn.ColumnName))
            : CreateUnsortedQueryParameters(columnSchemata);

        return Task.FromResult(queryParameters);
    }

    /// <inheritdoc />
    public async Task<RecordSet> ExecuteOffsetQuery(
        OffsetQueryDefinition queryDefinition,
        long limit,
        long offset,
        CancellationToken cancellationToken)
    {
        var query = new OffsetDataQuery(
            _logger,
            _connectionFactory,
            queryDefinition);

        var data = await query.ExecuteAsync(limit, offset, cancellationToken);

        return new RecordSet(queryDefinition.ColumnSchemata, data);
    }

    private OffsetQueryDefinition CreateUnsortedQueryParameters(IReadOnlyList<ColumnSchema> columnSchemata)
    {
        var sql = MyResouces
            .SortedDataQuery
            .Replace("$(columns)", string.Join(',', columnSchemata.Select(c => c.ColumnName)))
            .Replace("$(schema)", _dataSource.SchemaName)
            .Replace("$(table)", _dataSource.TableName);

        return new OffsetQueryDefinition(sql, columnSchemata);
    }

    private OffsetQueryDefinition CreateSortedQueryParameters(
        IReadOnlyList<ColumnSchema> columnSchemata,
        ColumnSchema sortColumn)
    {
        var sql = MyResouces
            .SortedDataQuery
            .Replace("$(columns)", string.Join(',', columnSchemata.Select(c => c.ColumnName)))
            .Replace("$(schema)", _dataSource.SchemaName)
            .Replace("$(table)", _dataSource.TableName)
            .Replace("$(sort-column)", _dataSource.SortColumn!.ColumnName);

        return new OffsetQueryDefinition(
            sql,
            columnSchemata,
            sortColumn);
    }
}