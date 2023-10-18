using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Vertical.DbExport.Services;

namespace Vertical.DbExport.IO;

public abstract class FileSystem : IFileSystem
{
    private readonly ILogger _logger;
    private readonly string _basePath;

    protected FileSystem(ILogger logger, string basePath)
    {
        _logger = logger;
        _basePath = basePath;
    }

    protected virtual string? Extension => null;

    public string MakePath(string path, bool createDirectory = false)
    {
        var fullPath = CreatePath(Path.Combine(_basePath, path));

        _logger.LogTrace("Resolved file system path {path}", fullPath);

        if (!createDirectory)
            return fullPath;

        var directory = Path.GetDirectoryName(fullPath) ?? throw new InvalidOperationException(
            $"Path {path} not rooted");

        if (Directory.Exists(directory))
            return fullPath;

        Directory.CreateDirectory(directory);
        _logger.LogTrace("Created directory {path}", directory);
        return fullPath;
    }

    /// <inheritdoc />
    public abstract Task<T> ReadFileAsync<T>(string path, Func<Stream, Task<T>> reader);

    /// <inheritdoc />
    public abstract Task<FileDescriptor> WriteFileAsync(string path, Func<Stream, Task> writer);

    /// <summary>
    /// Computes a file content hash
    /// </summary>
    /// <param name="path"></param>
    public static async Task<string> ComputeContentHashAsync(string path)
    {
        await using var stream = File.OpenRead(path);
        var hashBytes = await SHA1.HashDataAsync(stream);
        return Convert.ToHexString(hashBytes).ToLower();
    }

    public Stream CreateReadStream(string path)
    {
        var fullPath = MakePath(path);
        _logger.LogTrace("Opening readable file stream {path}", fullPath);
        return File.OpenRead(MakePath(fullPath));
    }

    protected Stream CreateWriteStream(string path)
    {
        var fullPath = MakePath(path, createDirectory: true);
        _logger.LogTrace("Opening writeable file stream {path}", fullPath);
        return File.OpenWrite(fullPath);
    }

    private string CreatePath(string path)
    {
        if (Path.IsPathRooted(path))
            return path;
        
        var replaced = Regex.Replace(path, @"\$\((\w+)\)", match =>
        {
            var pathEnum = Enum.Parse<Environment.SpecialFolder>(match.Groups[1].Value);
            return Environment.GetFolderPath(pathEnum);
        });
        
        if (Extension != null) replaced += Extension;

        return Path.GetFullPath(replaced);
    }
}