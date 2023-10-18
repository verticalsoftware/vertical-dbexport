using System.Data.SQLite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vertical.DbExport.Exceptions;
using Vertical.DbExport.Extensions;
using Vertical.DbExport.Pipeline;
using Vertical.DbExport.Resources;
using Vertical.DbExport.Services;

namespace Vertical.DbExport.Models;

[ServiceRegistration(ServiceLifetime.Scoped)]
public sealed class RestorePoint : IAsyncDisposable
{
    private const string SqlResourcesPath = "core.restore.txt";
    private readonly ILoggerFactory _loggerFactory;
    private readonly IFileSystemFactory _fileSystemFactory;
    private readonly ILogger<RestorePoint> _logger;
    private readonly SemaphoreSlim _lock = new(1);
    private SQLiteConnection? _connection;

    public RestorePoint(ILoggerFactory loggerFactory, IFileSystemFactory fileSystemFactory)
    {
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<RestorePoint>();
        _fileSystemFactory = fileSystemFactory;
    }

    public async Task InitializeAsync(JobContext context, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Initializing restore point database.");
        
        var fileSystem = _fileSystemFactory.CreateRestorePointMount(_loggerFactory, context.Job);
        var path = fileSystem.MakePath("data.sqlite", createDirectory: true);
        var connection = new SQLiteConnection($"Data Source={path}");
        
        await connection.OpenAsync(cancellationToken);
        await CreateSchemaAsync(connection);

        var jobHash = context.RestorePointSha;
        var restoreFileHash = await GetJobHashAsync(connection);

        if (restoreFileHash == null)
        {
            _logger.LogInformation("Creating new restore point.");

            var sql = StringResourceReader.GetResource(SqlResourcesPath, "insert-basic-info");
            await connection.ExecuteAndLogAsync(sql, new
            {
                jobName = context.Job.Name,
                jobHash = context.RestorePointSha
            }, _logger);
        }
        else if (restoreFileHash != jobHash)
        {
            _logger.LogError(
                "Could not use restore point for job {job} - options were changed (you can override this behavior with --force-restore",
                context.Job.Name);
            throw new CriticalStopException();
        }
        else
        {
            _logger.LogInformation("Restore point database initialized.");
        }

        _connection = connection;
    }
    
    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        _lock.Dispose();
        
        if (_connection == null)
            return;

        await _connection.DisposeAsync();
        _connection = null;
    }

    public async Task CompleteAsync(
        long partitionOffset,
        long countLeft,
        int queryPartitionId,
        int queryPartitionCount)
    {
        var sql = StringResourceReader.GetResource(SqlResourcesPath, "update-info");
        var param = new
        {
            partitionOffset,
            countLeft,
            queryPartitionId,
            queryPartitionCount
        };
        await ExecuteInLockAsync(async db => await db.ExecuteAndLogAsync(sql, param, _logger));
        _logger.LogInformation("Wrote restore point data.");
    }

    public async Task AddCachedQueryDescriptorAsync(CachedPartitionedFileDescriptor descriptor)
    {
        var sql = StringResourceReader.GetResource(SqlResourcesPath, "insert-cache-file");
        await ExecuteInLockAsync(async db => await db.ExecuteAndLogAsync(sql, descriptor, _logger));
    }

    public async Task<CachedPartitionedFileDescriptor?> GetQueryDescriptorAsync(long offset)
    {
        var sql = StringResourceReader.GetResource(SqlResourcesPath, "query-cache-file");
        var param = new { offset };
        var results = await ExecuteInLockAsync(async db => await db.QueryAndLogAsync<CachedPartitionedFileDescriptor>(
            sql, param, _logger));
        return results.FirstOrDefault();
    }

    public async Task<IReadOnlyList<CachedPartitionedFileDescriptor>> GetCachedQueryDescriptorsAsync()
    {
        var sql = StringResourceReader.GetResource(SqlResourcesPath, "query-cache-files");
        return await ExecuteInLockAsync(async db => await db.QueryAndLogAsync<CachedPartitionedFileDescriptor>(sql,
            null, _logger));
    }
    
    public async Task AddOutputFileAsync(OutputFileDescriptor descriptor)
    {
        var sql = StringResourceReader.GetResource(SqlResourcesPath, "insert-output-file");
        await ExecuteInLockAsync(async connection => await connection.ExecuteAndLogAsync(sql, descriptor, _logger));
    }

    public async Task<OutputFileDescriptor?> GetOutputFileAsync(long offsetStart, long offsetStop)
    {
        var sql = StringResourceReader.GetResource(SqlResourcesPath, "query-output-file");
        return await ExecuteInLockAsync(async connection =>
        {
            var result = await connection.QueryAndLogAsync<OutputFileDescriptor>(
                sql,
                new { offsetStart, offsetStop },
                _logger);
            return result.FirstOrDefault();
        });
    }

    private void ThrowIfConnectionNotInitialized()
    {
        if (_connection != null) return;
        throw new InvalidOperationException("Restore point SQLite connection not initialized");
    }

    private async Task<string?> GetJobHashAsync(SQLiteConnection connection)
    {
        var results = await connection.QueryAndLogAsync<dynamic>(
            StringResourceReader.GetResource(SqlResourcesPath, "job-hash-query"),
            null, _logger);
        
        return results.FirstOrDefault()?.JobHash;
    }

    private async Task CreateSchemaAsync(SQLiteConnection connection)
    {
        await connection.ExecuteAndLogAsync(
            StringResourceReader.GetResource(SqlResourcesPath, "create-schema"), 
            null, _logger);
    }

    private async Task<T> ExecuteInLockAsync<T>(Func<SQLiteConnection, Task<T>> command)
    {
        await _lock.WaitAsync();
        try
        {
            ThrowIfConnectionNotInitialized();
            return await command(_connection!);
        }
        finally
        {
            _lock.Release();
        }
    }
}