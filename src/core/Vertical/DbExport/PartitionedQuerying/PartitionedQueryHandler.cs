using Microsoft.Extensions.Logging;
using Vertical.DbExport.Exceptions;
using Vertical.DbExport.IO;
using Vertical.DbExport.Models;
using Vertical.DbExport.Services;

namespace Vertical.DbExport.PartitionedQuerying;

/// <summary>
/// Handles a partitioned query.
/// </summary>
public class PartitionedQueryHandler : IPartitionedQueryHandler
{
    private readonly ILogger _logger;
    private readonly PartitionedQuery _partitionedQuery;
    private readonly IDataChannel _dataChannel;
    private readonly RestorePoint _restorePoint;
    private readonly DataFileAggregator _dataFileAggregator;

    /// <summary>
    /// Creates a new instance of this type
    /// </summary>
    public PartitionedQueryHandler(
        ILogger logger,
        PartitionedQuery partitionedQuery,
        IDataChannel dataChannel,
        RestorePoint restorePoint,
        DataFileAggregator dataFileAggregator)
    {
        _logger = logger;
        _partitionedQuery = partitionedQuery;
        _dataChannel = dataChannel;
        _restorePoint = restorePoint;
        _dataFileAggregator = dataFileAggregator;
    }

    /// <inheritdoc />
    public async Task<long> ExecuteToChannelAsync(CancellationToken cancellationToken)
    {
        var cacheDescriptor = await ExecuteToChannelInternalAsync(cancellationToken);
        
        await _dataFileAggregator.EnqueueCachedQueryDataFileAsync(
            cacheDescriptor, 
            _partitionedQuery.JobContext, 
            cancellationToken);

        return cacheDescriptor.RecordCount;
    }

    private async Task<CachedPartitionedFileDescriptor> ExecuteToChannelInternalAsync(CancellationToken cancellationToken)
    {
        var cacheDescriptor = await _restorePoint.GetQueryDescriptorAsync(_partitionedQuery.RecordOffset);
        while (cacheDescriptor != null)
        {
            // File deleted, re-write
            if (!Path.Exists(cacheDescriptor.Path))
                break;

            var hash = await FileSystem.ComputeContentHashAsync(cacheDescriptor.Path);
            if (hash != cacheDescriptor.ContentHash)
            {
                _logger.LogError(
                    "Cached file {path} is corrupt or its content has changed since it was marked in restore point.",
                    cacheDescriptor.Path);
                throw new CriticalStopException();
            }
            
            _logger.LogDebug("Skipping partition query (using verified cached data file).");
            return cacheDescriptor;
        }
        
        var limit = _partitionedQuery.RecordLimit;
        var offset = _partitionedQuery.RecordOffset;
        var queryDefinition = _partitionedQuery.QueryDefinition;
        var context = _partitionedQuery.DataContext;
        var table = await context.ExecuteOffsetQuery(queryDefinition, limit , offset, cancellationToken);
        
        var fileDescriptor = await _dataChannel.WriteAsync(table);
        var contentHash = await FileSystem.ComputeContentHashAsync(fileDescriptor.Path);

        if (cacheDescriptor != null)
        {
            if (cacheDescriptor.ContentHash != contentHash)
            {
                throw new InvalidOperationException(
                    $"New cache file {cacheDescriptor.Path} hashed differently than what was recorded in the restore point..");
            }
            
            _logger.LogWarning("Cache file {path} recorded in restore point was rewritten. ", fileDescriptor.Path);
            return cacheDescriptor;
        }
        
        cacheDescriptor = new CachedPartitionedFileDescriptor
        {
            Compression = fileDescriptor.Compression,
            Offset = _partitionedQuery.RecordOffset,
            Path = fileDescriptor.Path,
            RecordCount = table.Rows.Count,
            SizeInBytes = fileDescriptor.SizeInBytes,
            ContentHash = contentHash
        };
        
        await _restorePoint.AddCachedQueryDescriptorAsync(cacheDescriptor);

        return cacheDescriptor;
    }
}