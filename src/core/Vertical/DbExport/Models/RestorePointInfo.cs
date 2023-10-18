namespace Vertical.DbExport.Models;

public class RestorePointInfo
{
    public required string JobName { get; init; }
    
    public required string JobHash { get; init; }
    
    public long PartitionOffset { get; set; }
    
    public long CountLeft { get; set; }
    
    public int QueryPartitionId { get; set; }
    
    public int QueryPartitionCount { get; set; }

    public List<CachedPartitionedFileDescriptor> CacheFiles { get; set; } = new();

    public List<string> Exceptions { get; } = new();
}