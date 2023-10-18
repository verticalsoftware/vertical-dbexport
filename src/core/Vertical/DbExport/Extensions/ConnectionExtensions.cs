using System.Data.Common;
using Dapper;
using Microsoft.Extensions.Logging;
using Vertical.DbExport.Models;

namespace Vertical.DbExport.Extensions;

public static class ConnectionExtensions
{
    public static async Task<IReadOnlyList<Record>> QueryAndLogAsync(
        this DbConnection connection,
        string sql,
        object? param,
        ILogger logger)
    {
        logger.LogTrace("Executing sql query on {datasource}:\n{sql}\nparameters={@parameters}",
            $"{connection.GetType().Name}:{connection.DataSource}",
            sql,
            param);

        var data = (await connection.QueryAsync(sql, param: param)).ToArray();
        
        logger.LogTrace("Query result = {count} rows", data.Length);

        return data.Select(obj => new Record(obj)).ToArray();
    }
    
    public static async Task<IReadOnlyList<T>> QueryAndLogAsync<T>(
        this DbConnection connection,
        string sql,
        object? param,
        ILogger logger)
    {
        logger.LogTrace("Executing sql query on {datasource}:\n{sql}\nparameters={@parameters}",
            $"{connection.GetType().Name}:{connection.DataSource}",
            sql,
            param);

        var data = (await connection.QueryAsync<T>(sql, param: param)).ToArray();
        
        logger.LogTrace("Query result = {count} rows", data.Length);

        return data;
    }

    public static async Task<int> ExecuteAndLogAsync(
        this DbConnection connection,
        string sql,
        object? param,
        ILogger logger)
    {
        logger.LogTrace("Executing sql query on {datasource}:\n{sql}\nparameters={@parameters}",
            $"{connection.GetType().Name}:{connection.DataSource}",
            sql,
            param);

        var result = (await connection.ExecuteAsync(sql, param: param));
        
        logger.LogTrace("Execute result = {count} rows", result);

        return result;
    }
}