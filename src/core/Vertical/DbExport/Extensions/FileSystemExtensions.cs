using Vertical.DbExport.Services;

namespace Vertical.DbExport.Extensions;

public static class FileSystemExtensions
{
    public static async Task<T?> TryReadFileAsync<T>(
        this IFileSystem fileSystem,
        string path,
        Func<Stream, Task<T>> reader)
    {
        try
        {
            return await fileSystem.ReadFileAsync(path, reader);
        }
        catch (IOException)
        {
            return default;
        }
    }
}