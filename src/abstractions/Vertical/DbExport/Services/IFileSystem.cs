using Vertical.DbExport.IO;

namespace Vertical.DbExport.Services;

/// <summary>
/// Abstracts the file system.
/// </summary>
public interface IFileSystem
{
    /// <summary>
    /// Creates a read stream.
    /// </summary>
    /// <param name="path">Path</param>
    /// <returns>Stream</returns>
    Stream CreateReadStream(string path);
    
    /// <summary>
    /// Reads file content.
    /// </summary>
    /// <param name="path">File path</param>
    /// <param name="reader">Function that reads content from a stream.</param>
    /// <typeparam name="T">Data type</typeparam>
    /// <returns>Task that completes will the file content</returns>
    Task<T> ReadFileAsync<T>(string path, Func<Stream, Task<T>> reader);

    /// <summary>
    /// Writes a file.
    /// </summary>
    /// <param name="path">File path</param>
    /// <param name="writer">Function that writes content to the stream.</param>
    /// <returns><see cref="FileInfo"/></returns>
    Task<FileDescriptor> WriteFileAsync(string path, Func<Stream, Task> writer);
    
    /// <summary>
    /// Creates a path string, and optionally creates the underlying directory.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="createDirectory"></param>
    /// <returns></returns>
    string MakePath(string path, bool createDirectory = false);
}