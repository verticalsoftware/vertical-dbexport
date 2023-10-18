using Microsoft.Extensions.Logging;
using MySqlConnector;
using Vertical.DbExport.Data;
using Vertical.DbExport.Extensions;
using Vertical.DbExport.Models;

namespace Vertical.DbExport.MySql;

public class ColumnSchemaQuery
{
    private readonly ILogger _logger;
    private readonly ConnectionFactory<MySqlConnection> _connectionFactory;
    private readonly string _table;

    public ColumnSchemaQuery(ILogger logger,
        ConnectionFactory<MySqlConnection> connectionFactory,
        string table)
    {
        _logger = logger;
        _connectionFactory = connectionFactory;
        _table = table;
    }
    
    public async Task<IReadOnlyList<ColumnSchema>> ExecuteAsync(CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory(cancellationToken);

        var sql = MyResouces.ColumnSchemaQuery;
        var parameters = new
        {
            schema = connection.Database,
            table = _table
        };

        var data = await connection.QueryAndLogAsync(sql, parameters, _logger);
        return data.Select(rec => CreateColumnSchema(rec)).ToArray();
    }

    private static ColumnSchema CreateColumnSchema(in Record row)
    {
        return new ColumnSchema
        {
            ColumnName = row.GetRequiredValue<string>("COLUMN_NAME"),
            OrdinalPosition = Convert.ToInt32(row.GetRequiredValue<uint>("ORDINAL_POSITION")),
            DataType = row.GetSchemaFieldType(),
            IsNullable = row.IsNullableSchemaField(),
            MaxLength =  row.GetValue<long, int>("CHARACTER_MAXIMUM_LENGTH", Convert.ToInt32),
            KeyType = row.GetSchemaKeyType()
        };
    }
}