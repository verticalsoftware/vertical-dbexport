using System.IO.Compression;
using Microsoft.Extensions.Logging;
using Vertical.DbExport.Options;

namespace Vertical.DbExport.IO;

public class DiskCompressedFileSystem : FileSystem
{
    /// <summary>
    /// Creates a new instance of this type
    /// </summary>
    public DiskCompressedFileSystem(ILogger logger, string basePath) : base(logger, basePath)
    {
    }

    /// <inheritdoc />
    protected override string? Extension => ".gz";

    /// <inheritdoc />
    public override async Task<T> ReadFileAsync<T>(string path, Func<Stream, Task<T>> reader)
    {
        await using var fileStream = CreateReadStream(path);
        await using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);

        return await reader(gzipStream);
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
            Compression = OutputFileCompression.GZip
        };
    }

    private async Task WriteFileInternalAsync(string path, Func<Stream, Task> writer)
    {
        await using var fileStream = CreateWriteStream(path);
        await using var gzipStream = new GZipStream(fileStream, CompressionMode.Compress);
        await writer(gzipStream);
    }
}