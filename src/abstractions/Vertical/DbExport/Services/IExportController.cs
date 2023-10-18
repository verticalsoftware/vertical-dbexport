using Vertical.DbExport.Options;

namespace Vertical.DbExport.Services;

/// <summary>
/// Represents the export controller.
/// </summary>
public interface IExportController
{
    /// <summary>
    /// Executes the export.
    /// </summary>
    /// <param name="options">Options</param>
    /// <param name="cancellationToken">A token that can be monitored for cancellation requests</param>
    /// <returns>Task</returns>
    Task ExecuteAsync(ExportOptions options, CancellationToken cancellationToken);
}