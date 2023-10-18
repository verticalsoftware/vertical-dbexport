namespace Vertical.DbExport.Options;

/// <summary>
/// Represents options for the core loader.
/// </summary>
public class ConnectionOptions
{
    /// <summary>
    /// Gets the database specific driver to use.
    /// </summary>
    public string Driver { get; set; } = default!;

    /// <summary>
    /// Gets the connection string used to connect to the database.
    /// </summary>
    public string ConnectionString { get; set; } = default!;

    /// <summary>
    /// Gets the timeout for connection attempts in seconds.
    /// </summary>
    public uint ConnectionTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets the command timeout in seconds.
    /// </summary>
    public uint CommandTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets the number of times to retry connection.
    /// </summary>
    public int ConnectionRetryCount { get; set; } = 3;

    /// <summary>
    /// Gets the number of times to retry query commands.
    /// </summary>
    public int CommandRetryCount { get; set; } = 3;
}