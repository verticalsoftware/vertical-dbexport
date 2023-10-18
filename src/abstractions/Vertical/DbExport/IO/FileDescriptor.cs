using Vertical.DbExport.Options;

namespace Vertical.DbExport.IO;

public class FileDescriptor
{
    public required string Path { get; init; }
    public required long SizeInBytes { get; init; }
    public required OutputFileCompression Compression { get; init; }
}