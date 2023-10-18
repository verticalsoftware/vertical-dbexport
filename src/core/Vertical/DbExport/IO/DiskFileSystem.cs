using Microsoft.Extensions.Logging;
using Vertical.DbExport.Options;

namespace Vertical.DbExport.IO;

public class DiskFileSystem : FileSystem
{

    /// <summary>
    /// Creates a new instance of this type
    /// </summary>
    public DiskFileSystem(ILogger logger, string basePath) : base(logger, basePath)
    {
    }

    /// <inheritdoc />
    public override async Task<T> ReadFileAsync<T>(string path, Func<Stream, Task<T>> reader)
    {
        await using var fileStream = CreateReadStream(path);
        return await reader(fileStream);
    }

    /// <inheritdoc />
    public override async Task<FileDescriptor> WriteFileAsync(string path, Func<Stream, Task> writer)
    {
        await WriteFileInternalAsync(path, writer);
        var fileInfo = new FileInfo(MakePath(path));

        return new FileDescriptor
        {
            Path = fileInfo.FullName,
            SizeInBytes = fileInfo.Length,
            Compression = OutputFileCompression.None
        };
    }

    private async Task WriteFileInternalAsync(string path, Func<Stream, Task> writer)
    {
        await using var fileStream = CreateWriteStream(path);
        await writer(fileStream);
    }
}