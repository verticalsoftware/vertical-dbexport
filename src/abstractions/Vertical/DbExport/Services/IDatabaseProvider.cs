using Vertical.DbExport.Models;
using Vertical.DbExport.Options;


namespace Vertical.DbExport.Services;

/// <summary>
/// Represents a database provider.
/// </summary>
public interface IDatabaseProvider
{
    /// <summary>
    /// Gets the provider id.
    /// </summary>
    string DriverId { get; }
    
    /// <summary>
    /// Gets or sets connection options.
    /// </summary>
    ConnectionOptions? ConnectionOptions { get; set; }

    /// <summary>
    /// Validates the provider connection.
    /// </summary>
    /// <param name="cancellationToken">A token that can be monitored for cancellation requests</param>
    /// <returns>Task</returns>
    Task ValidateAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Creates a database context.
    /// </summary>
    /// <param name="dataSource">Data source</param>
    /// <returns><see cref="IDataContext"/></returns>
    IDataContext CreateContext(DataSourceOptions dataSource);
}