namespace Vertical.DbExport.Options;

/// <summary>
/// Represent export options.
/// </summary>
public class ExportOptions
{
    /// <summary>
    /// Gets additional metadata that describes the export.
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
    
    /// <summary>
    /// Gets connection options.
    /// </summary>
    public ConnectionOptions Connection { get; set; } = default!;

    /// <summary>
    /// Gets the jobs.
    /// </summary>
    public JobOptions[] Jobs { get; set; } = Array.Empty<JobOptions>();
}