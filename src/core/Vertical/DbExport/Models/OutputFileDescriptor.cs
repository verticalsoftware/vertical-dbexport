using Vertical.DbExport.IO;

namespace Vertical.DbExport.Models;

public class OutputFileDescriptor : FileDescriptor
{
    public required long OffsetStart { get; set; }
    
    public required long OffsetStop { get; set; }
    
    public required long RecordCount { get; set; }
    
    public required string ContentHash { get; set; }
}