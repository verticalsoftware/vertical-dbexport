using Vertical.DbExport.Models;

namespace Vertical.DbExport.Services;

/// <summary>
/// Represents a gateway to data functionality of a database.
/// </summary>
public interface IDataContext
{
    /// <summary>
    /// Gets the column schema.
    /// </summary>
    /// <param name="cancellationToken">A token that can be monitored for cancellation requests</param>
    /// <returns>Column schema list</returns>
    Task<IReadOnlyList<ColumnSchema>> GetColumnSchemaAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Creates a reusable set of offset query parameters.
    /// </summary>
    /// <param name="columnSchemata">Column schemata</param>
    /// <param name="cancellationToken">A token that can be monitored for cancellation requests</param>
    /// <returns><see cref="OffsetQueryDefinition"/></returns>
    Task<OffsetQueryDefinition> CreateOffsetQueryDefinition(
        IReadOnlyList<ColumnSchema> columnSchemata,
        CancellationToken cancellationToken);

    /// <summary>
    /// Executes an offset query.
    /// </summary>
    /// <param name="queryDefinition">Query definition.</param>
    /// <param name="offset">Offset count</param>
    /// <param name="cancellationToken">A token that can be monitored for cancellation requests</param>
    /// <param name="limit">Limit count</param>
    /// <returns><see cref="RecordSet"/></returns>
    Task<RecordSet> ExecuteOffsetQuery(
        OffsetQueryDefinition queryDefinition,
        long limit,
        long offset,
        CancellationToken cancellationToken);
}