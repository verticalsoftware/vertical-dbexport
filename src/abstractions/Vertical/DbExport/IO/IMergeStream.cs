using Vertical.DbExport.Services;

namespace Vertical.DbExport.IO;

public interface IMergeStream
{
    Task AppendAsync(
        Stream stream,
        long startIndex,
        long count,
        CancellationToken cancellationToken);

    Task<FileDescriptor> FlushAsync(IFileSystem fileSystem, string path, CancellationToken cancellationToken);
}