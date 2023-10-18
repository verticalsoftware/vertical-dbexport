using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Vertical.DbExport.Extensions;
using Vertical.DbExport.Models;
using Vertical.DbExport.Options;
using Vertical.DbExport.PartitionedQuerying;
using Vertical.DbExport.Services;
using Vertical.DbExport.Threading;
using Vertical.Pipelines;

namespace Vertical.DbExport.Pipeline.Jobs;

public class RunPartitionedQueriesTask : IPipelineMiddleware<JobContext>
{
    private record ChannelMonitor(
        CancellationTokenSource InnerCancellationSource,
        CancellationToken PrimeCancellationToken,
        VolatileValue<long> RowCount)
    {
        internal ConcurrentStack<Exception> Exceptions { get; } = new();
    };

    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<RunPartitionedQueriesTask> _logger;
    private readonly PartitionedQueryHandlerFactory _partitionedQueryHandlerFactory;
    private readonly IFileSystemFactory _fileSystemFactory;
    private readonly RestorePoint _restorePoint;

    /// <summary>
    /// Creates a new instance of this type
    /// </summary>
    public RunPartitionedQueriesTask(
        ILoggerFactory loggerFactory,
        PartitionedQueryHandlerFactory partitionedQueryHandlerFactory,
        IFileSystemFactory fileSystemFactory,
        RestorePoint restorePoint)
    {
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<RunPartitionedQueriesTask>();
        _partitionedQueryHandlerFactory = partitionedQueryHandlerFactory;
        _fileSystemFactory = fileSystemFactory;
        _restorePoint = restorePoint;
    }

    /// <inheritdoc />
    public async Task InvokeAsync(
        JobContext context,
        PipelineDelegate<JobContext> next,
        CancellationToken cancellationToken)
    {
        try
        {
            await _restorePoint.InitializeAsync(context, cancellationToken);
            _logger.LogInformation("Executing partitioned queries.");
            await ExecutePartitioningAsync(context, cancellationToken);
        }
        catch (TaskCanceledException)
        {
        }

        await next(context, cancellationToken);
    }
    
    private async Task ExecutePartitioningAsync(
        JobContext context,
        CancellationToken cancellationToken)
    {
        using var innerCancellationSource = new CancellationTokenSource();
        using var linkedCancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            innerCancellationSource.Token,
            cancellationToken);

        var fileSystem = _fileSystemFactory.CreatePartitionedQueryMount(
            _loggerFactory,
            context.Job,
            context.Job.Output.Compression);
        var dataContext = context.DatabaseProvider.CreateContext(context.Job.DataSource);
        var queryDefinition = await dataContext.CreateOffsetQueryDefinition(context.ColumnSchemata,
            cancellationToken);
        var monitor = new ChannelMonitor(
            innerCancellationSource, 
            cancellationToken,
            new VolatileValue<long>(0L));
        var threads = context.Job.Parallelization.MaxPartitionThreads;
        var partitionOffset = 0L;
        var countLeft = context.Job.Constraints.MaxRowCount.GetValueOrDefault(long.MaxValue);
        var batchSize = context.Job.Parallelization.QueryBatchSize;
        var partitionId = 0;
        var partitionedQueryCount = 0;
        var jobHash = context.Job.Sha();
        var channel = Channel.CreateBounded<PartitionedQuery>(threads);
        var channelReaders = Enumerable
            .Range(0, threads)
            .Select(i => ReadPartitionedQueryChannelAsync(i, channel.Reader, monitor))
            .ToArray();
        
        _logger.LogDebug("Started {count} partitioned query threads.", threads);

        while (countLeft > 0 && !linkedCancelTokenSource.IsCancellationRequested)
        {
            var partitionQuery = new PartitionedQuery
            {
                PartitionId = partitionId++,
                Connection = context.Options.Connection,
                ColumnSchema = context.ColumnSchemata,
                DatabaseProvider = context.DatabaseProvider,
                DataContext = dataContext,
                QueryDefinition = queryDefinition,
                JobContext = context,
                RecordLimit = Math.Min(batchSize, countLeft),
                RecordOffset = partitionOffset,
                SortColumnSchema = context.SortColumnSchema,
                FileSystem = fileSystem,
                RestorePointHash = jobHash
            };

            partitionOffset += context.Job.Parallelization.QueryBatchSize; 
            countLeft -= batchSize;
            partitionedQueryCount++;

            _logger.LogDebug("Enqueuing partitioned query (id={id}, limit={limit}, offset={offset})",
                partitionId,
                partitionQuery.RecordLimit,
                partitionQuery.RecordOffset);
            
            await channel.Writer.WriteAsync(partitionQuery, linkedCancelTokenSource.Token);
        }

        channel.Writer.Complete();
        await Task.WhenAll(channelReaders);

        var exceptions = monitor.Exceptions.ToArray();
        if (exceptions.Length > 0)
        {
            throw new AggregateException(exceptions);
        }
        
        await _restorePoint.CompleteAsync(partitionOffset, countLeft, partitionedQueryCount, partitionedQueryCount);
        
        _logger.LogInformation("Wrote {rows} rows over {count} partitioned queries to cache.",
            monitor.RowCount.Value,
            partitionedQueryCount);
    }

    private async Task ReadPartitionedQueryChannelAsync(
        int partitionIndex,
        ChannelReader<PartitionedQuery> channelReader,
        ChannelMonitor channelMonitor)
    {
        using var _ = _logger.BeginScope("Partition {index}/Thread {threadId}", partitionIndex, Environment.CurrentManagedThreadId);
        _logger.LogDebug("Query partition thread entering");

        try
        {
            await foreach (var partitionedQuery in channelReader.ReadAllAsync(channelMonitor.PrimeCancellationToken))
            {
                var count = await ExecutePartitionedQueryAsync(partitionedQuery, channelMonitor.PrimeCancellationToken);
                channelMonitor.RowCount.Exchange(previous => previous + count);
                if (count >= partitionedQuery.Job.Parallelization.QueryBatchSize)
                    continue;
                
                channelMonitor.InnerCancellationSource.Cancel();
                
                // Query returned less than batch size... we're at the end of the data source
                _logger.LogDebug("Partial query result count {count} < batch size {batchSize}, signaling partitioning stop",
                    count,
                    partitionedQuery.Job.Parallelization.QueryBatchSize);

                break;
            }
            _logger.LogDebug("Query partition thread exiting.");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error occurred during execution of partitioned query");
            channelMonitor.InnerCancellationSource.Cancel();
            channelMonitor.Exceptions.Push(exception);
        }
    }

    private async Task<long> ExecutePartitionedQueryAsync(
        PartitionedQuery partitionedQuery,
        CancellationToken cancellationToken)
    {
        var handler = _partitionedQueryHandlerFactory.CreateHandler(partitionedQuery);
        var stopWatch = Stopwatch.StartNew();
        var count = await handler.ExecuteToChannelAsync(cancellationToken);
            
        _logger.LogInformation("Retrieved {count} rows ({ms}ms) from offset {offset}", 
            count, 
            stopWatch.ElapsedMilliseconds,
            partitionedQuery.RecordOffset);

        return count;
    }
}