using Vertical.DbExport.Models;

namespace Vertical.DbExport.IO;

/// <summary>
/// Represents 
/// </summary>
public interface IDataChannel
{
    string FileExtension { get; }
    
    Task<FileDescriptor> WriteAsync(RecordSet set);

    IMergeStream CreateMergeStream(IReadOnlyList<ColumnSchema> columnSchemata, long capacity);
}