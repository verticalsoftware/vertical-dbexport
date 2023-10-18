using Microsoft.Extensions.Logging;
using Vertical.DbExport.Options;

namespace Vertical.DbExport.Services;

public interface IFileSystemFactory
{
    IFileSystem CreatePartitionedQueryMount(ILoggerFactory loggerFactory, JobOptions job, OutputFileCompression compression);

    IFileSystem CreateOutputMount(ILoggerFactory loggerFactory, JobOptions job, OutputFileCompression compression);

    IFileSystem CreateRestorePointMount(ILoggerFactory loggerFactory, JobOptions job);
}