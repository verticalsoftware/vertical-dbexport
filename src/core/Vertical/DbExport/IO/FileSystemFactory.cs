using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vertical.DbExport.Options;
using Vertical.DbExport.Services;

namespace Vertical.DbExport.IO;

[ServiceRegistration(ServiceLifetime.Singleton)]
public class FileSystemFactory : IFileSystemFactory
{
    private readonly ILogger<FileSystemFactory> _logger;

    /// <summary>
    /// Creates a new instance of this type
    /// </summary>
    public FileSystemFactory(ILogger<FileSystemFactory> logger)
    {
        _logger = logger;
    }
    
    /// <inheritdoc />
    public IFileSystem CreatePartitionedQueryMount(
        ILoggerFactory loggerFactory, 
        JobOptions job,
        OutputFileCompression compression)
    {
        return CreateInternal(loggerFactory, job, ".cache", compression);
    }

    /// <inheritdoc />
    public IFileSystem CreateOutputMount(ILoggerFactory loggerFactory, JobOptions job, OutputFileCompression compression)
    {
        return CreateInternal(loggerFactory, job, null, compression);
    }

    /// <inheritdoc />
    public IFileSystem CreateRestorePointMount(ILoggerFactory loggerFactory, JobOptions job)
    {
        return CreateInternal(loggerFactory, job, ".restore", OutputFileCompression.None);
    }

    private IFileSystem CreateInternal(ILoggerFactory loggerFactory, 
        JobOptions job,
        string? path, 
        OutputFileCompression compression)
    {
        var basePath = job.Output.Path ?? Directory.GetCurrentDirectory();
        var finalPath = !string.IsNullOrWhiteSpace(path)
            ? Path.Combine(basePath, path)
            : basePath;

        _logger.LogTrace("Creating file system for path {path}, compression={compression}",
            path,
            compression);
        
        return compression == OutputFileCompression.GZip
            ? new DiskCompressedFileSystem(loggerFactory.CreateLogger<DiskCompressedFileSystem>(), finalPath)
            : new DiskFileSystem(loggerFactory.CreateLogger<DiskFileSystem>(), finalPath);
    }
}