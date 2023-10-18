using Vertical.DbExport.Models;
using Vertical.DbExport.Options;
using Vertical.DbExport.Pipeline;
using Vertical.DbExport.Services;

namespace Vertical.DbExport.PartitionedQuerying;

public class PartitionedQuery
{
    public required ConnectionOptions Connection { get; init; }
    
    public required int PartitionId { get; init; }
    
    public required JobContext JobContext { get; init; }

    public JobOptions Job => JobContext.Job;
    
    public required IDatabaseProvider DatabaseProvider { get; init; }
    
    public required IDataContext DataContext { get; init; }
    
    public required string RestorePointHash { get; init; }
    
    public required long RecordOffset { get; init; }
    
    public required long RecordLimit { get; init; }
    
    public required IReadOnlyList<ColumnSchema> ColumnSchema { get; init; }
    
    public required ColumnSchema? SortColumnSchema { get; init; }
    
    public required OffsetQueryDefinition QueryDefinition { get; init; }
    
    public required IFileSystem FileSystem { get; init; }
}