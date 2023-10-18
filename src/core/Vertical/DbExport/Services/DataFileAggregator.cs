using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vertical.DbExport.Exceptions;
using Vertical.DbExport.IO;
using Vertical.DbExport.Models;
using Vertical.DbExport.Pipeline;

namespace Vertical.DbExport.Services;

[ServiceRegistration(ServiceLifetime.Scoped)]
public class DataFileAggregator
{
    private record StagedChunkEntry(
        CachedPartitionedFileDescriptor CachedFileDescriptor,
        long SelectStartIndex,
        long SelectCount,
        long ChunkSizeBytes);

    private record ChannelMessage(CachedPartitionedFileDescriptor CachedFileDescriptor, JobContext JobContext);
    
    private readonly ILogger<DataFileAggregator> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IFileSystemFactory _fileSystemFactory;
    private readonly IDataChannelFactory _dataChannelFactory;
    private readonly RestorePoint _restorePoint;
    private readonly Dictionary<long, CachedPartitionedFileDescriptor> _keyedDescriptorQueue = new(32);
    private readonly Channel<ChannelMessage> _channel = Channel.CreateBounded<ChannelMessage>(1);
    private readonly List<StagedChunkEntry> _stagedChunks = new(32);
    private readonly Task _fileWriterTask;
    private int _nextOutputFileId;
    private long _trackedSequenceOffset;
    private long _chunkWriteOffset;

    public DataFileAggregator(
        ILoggerFactory loggerFactory,
        IFileSystemFactory fileSystemFactory,
        IDataChannelFactory dataChannelFactory,
        RestorePoint restorePoint)
    {
        _logger = loggerFactory.CreateLogger<DataFileAggregator>();
        _loggerFactory = loggerFactory;
        _fileSystemFactory = fileSystemFactory;
        _dataChannelFactory = dataChannelFactory;
        _restorePoint = restorePoint;
        _fileWriterTask = ReadChannelAsync();
    }
    
    public async Task EnqueueCachedQueryDataFileAsync(
        CachedPartitionedFileDescriptor descriptor,
        JobContext jobContext,
        CancellationToken cancellationToken)
    {
        await _channel.Writer.WriteAsync(new ChannelMessage(descriptor, jobContext), cancellationToken);
    }

    public async Task FlushAsync()
    {
        _channel.Writer.Complete();
        await _fileWriterTask;
    }

    private async Task ReadChannelAsync()
    {
        JobContext? jobContext = null;
        
        await foreach (var message in _channel.Reader.ReadAllAsync())
        {
            _keyedDescriptorQueue.Add(message.CachedFileDescriptor.Offset, message.CachedFileDescriptor);
            await StageChunksAsync(jobContext = message.JobContext, CancellationToken.None);
        }

        if (jobContext != null)
        {
            await FlushStagedChunksAsync(jobContext, CancellationToken.None);
        }
    }

    private async Task StageChunksAsync(
        JobContext jobContext,
        CancellationToken cancellationToken)
    {
        var maxFileSize = jobContext.Job.Output.GetFileSizeInBytes();
        var bufferSize = maxFileSize - _stagedChunks.Sum(chunk => chunk.ChunkSizeBytes);
        var orderedDescriptor = DequeueOrderedCacheDescriptors(jobContext.Job.Parallelization.QueryBatchSize);

        _logger.LogTrace("Initializing buffer size to {bytes} bytes for next staging operation.", bufferSize);
        
        foreach (var descriptor in orderedDescriptor)
        {
            _logger.LogTrace("Enqueuing cache file {path} (offset={offset}, recordCount={count}).", 
                descriptor.Path, 
                descriptor.Offset, 
                descriptor.RecordCount);
            
            var bytesPerRecord = descriptor.SizeInBytes / descriptor.RecordCount;
            var selectOffset = 0L;
            var remainingRecordCount = descriptor.RecordCount;

            while (remainingRecordCount > 0)
            {
                var requiredChunkSize = bytesPerRecord * remainingRecordCount;
                if (bufferSize - requiredChunkSize > 0)
                {
                    _logger.LogTrace("Staging chunk recordOffset={offset}, count={count} with {size} bytes.",
                        selectOffset,
                        remainingRecordCount,
                        requiredChunkSize);
                    _stagedChunks.Add(new StagedChunkEntry(descriptor, selectOffset, remainingRecordCount,
                        requiredChunkSize));
                    bufferSize -= requiredChunkSize;
                    break;
                }

                var selectCount = bufferSize / bytesPerRecord;
                var partialChunk = new StagedChunkEntry(descriptor,
                    selectOffset,
                    selectCount,
                    selectCount * bytesPerRecord);
                
                _logger.LogTrace("Staging partial chunk recordOffset={offset}, count={count} with {size} bytes.",
                    partialChunk.SelectStartIndex,
                    partialChunk.SelectCount,
                    partialChunk.ChunkSizeBytes);
                
                _stagedChunks.Add(partialChunk);
                await FlushStagedChunksAsync(jobContext, cancellationToken);
                
                bufferSize = maxFileSize;
                remainingRecordCount -= selectCount;
                selectOffset += partialChunk.SelectCount;
                
                _logger.LogTrace("Reset chunk buffer space to {bytes} bytes.", bufferSize);
                
                _logger.LogTrace("Iterating partial buffer (size={bytes}, remaining records={count}, new offset={offset}",
                    bufferSize,
                    remainingRecordCount,
                    selectOffset);
            }
        }
    }

    private async Task FlushStagedChunksAsync(JobContext jobContext, CancellationToken cancellationToken)
    {
        if (_stagedChunks.Count == 0)
            return;
        
        var mergeEntries = _stagedChunks.Sum(chunk => chunk.SelectCount);
        var compression = jobContext.Job.Output.Compression;
        var readFileSystem = _fileSystemFactory.CreatePartitionedQueryMount(_loggerFactory, jobContext.Job, compression);
        var writeFileSystem = _fileSystemFactory.CreateOutputMount(_loggerFactory, jobContext.Job, compression);
        var dataChannel = _dataChannelFactory.CreateChannel(readFileSystem, "");
        var mergeStream = dataChannel.CreateMergeStream(jobContext.ColumnSchemata, mergeEntries);
        var rowsWritten = 0L;

        var queuedRows = _stagedChunks.Sum(chunk => chunk.SelectCount);
        var restoreEntry = await _restorePoint.GetOutputFileAsync(_chunkWriteOffset, _chunkWriteOffset + queuedRows);
        if (restoreEntry != null)
        {
            var computedHash = await FileSystem.ComputeContentHashAsync(restoreEntry.Path);
            if (computedHash != restoreEntry.ContentHash)
            {
                _logger.LogError("Data file {path} is corrupt (hash check failed).", restoreEntry.Path);
                throw new CriticalStopException();
            }
            
            _logger.LogInformation("Data file {path} is intact and hash check passed, skipping merge.",
                restoreEntry.Path);
            _chunkWriteOffset += queuedRows;
            _logger.LogDebug("Cleared staged chunks.");
            _stagedChunks.Clear();
            return;
        }
        
        foreach (var chunk in _stagedChunks)
        {
            await using var stream = readFileSystem.CreateReadStream(chunk.CachedFileDescriptor.Path);
            await mergeStream.AppendAsync(stream, chunk.SelectStartIndex, chunk.SelectCount, cancellationToken);
            rowsWritten += chunk.SelectCount;
        }

        var descriptor = await mergeStream.FlushAsync(writeFileSystem, GetOutputFileName(jobContext, _nextOutputFileId++),
            cancellationToken);

        var hashCode = await FileSystem.ComputeContentHashAsync(descriptor.Path);

        await _restorePoint.AddOutputFileAsync(new OutputFileDescriptor
        {
            Path = descriptor.Path,
            Compression = descriptor.Compression,
            SizeInBytes = descriptor.SizeInBytes,
            OffsetStart = _chunkWriteOffset,
            OffsetStop = _chunkWriteOffset + rowsWritten,
            RecordCount = rowsWritten,
            ContentHash = hashCode
        });

        _chunkWriteOffset += rowsWritten;
        
        _logger.LogInformation("Wrote data file {path} ({count} bytes).", descriptor.Path, descriptor.SizeInBytes);
        
        _stagedChunks.Clear();
        _logger.LogDebug("Cleared staged chunks.");
    }

    private IReadOnlyList<CachedPartitionedFileDescriptor> DequeueOrderedCacheDescriptors(int batchSize)
    {
        var list = new List<CachedPartitionedFileDescriptor>(32);

        while (_keyedDescriptorQueue.TryGetValue(_trackedSequenceOffset, out var descriptor))
        {
            list.Add(descriptor);
            _trackedSequenceOffset += batchSize;
            _keyedDescriptorQueue.Remove(descriptor.Offset);
        }

        return list;
    }

    private static string GetOutputFileName(JobContext jobContext, int sequence)
    {
        return jobContext.Job.Output.FileNameTemplate
            .Replace("$(table)", jobContext.Job.DataSource.TableName)
            .Replace("$(sequence)", $"{sequence:00000}");
    }
}