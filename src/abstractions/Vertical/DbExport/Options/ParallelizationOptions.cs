namespace Vertical.DbExport.Options;

/// <summary>
/// Represents options for partitioning.
/// </summary>
public class ParallelizationOptions
{
    /// <summary>
    /// Gets the number of records to retrieve within a query partition.
    /// </summary>
    public int QueryBatchSize { get; set; } = 5000;
    
    /// <summary>
    /// Gets the number of records to allow in a parallel partition.
    /// </summary>
    public long PartitionSize { get; set; } = 25000;

    /// <summary>
    /// Gets the max partition threads.
    /// </summary>
    public int MaxPartitionThreads { get; set; } = 5;
}