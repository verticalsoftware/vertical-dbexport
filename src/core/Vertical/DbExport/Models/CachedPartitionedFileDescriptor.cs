using Vertical.DbExport.IO;

namespace Vertical.DbExport.Models;

public class CachedPartitionedFileDescriptor : FileDescriptor
{
    public required long Offset { get; set; }
    
    public required long RecordCount { get; set; }
    
    public required string ContentHash { get; set; }
    
    public int? OutputDataFileId { get; set; }
    
    public string? OutputDataFilePath { get; set; }
}