namespace Vertical.DbExport.Options;

/// <summary>
/// Represents options for a job.
/// </summary>
public class JobOptions
{
    /// <summary>
    /// Gets the job name.
    /// </summary>
    public string Name { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Gets the data source options.
    /// </summary>
    public DataSourceOptions DataSource { get; set; } = default!;

    /// <summary>
    /// Gets parallelization options.
    /// </summary>
    public ParallelizationOptions Parallelization { get; set; } = new();
    
    /// <summary>
    /// Gets the output options.
    /// </summary>
    public OutputOptions Output { get; set; } = new();

    /// <summary>
    /// Gets the constraints.
    /// </summary>
    public JobConstraintOptions Constraints { get; set; } = new();
}