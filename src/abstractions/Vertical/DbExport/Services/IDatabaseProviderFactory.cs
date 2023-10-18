using Vertical.DbExport.Options;

namespace Vertical.DbExport.Services;

/// <summary>
/// Provides database factory types.
/// </summary>
public interface IDatabaseProviderFactory
{
    /// <summary>
    /// Gets a database provider.
    /// </summary>
    /// <param name="connectionOptions">Connection options</param>
    /// <returns><see cref="IDatabaseProvider"/></returns>
    IDatabaseProvider GetProvider(ConnectionOptions connectionOptions);
}