using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vertical.DbExport.IO;
using Vertical.DbExport.Models;
using Vertical.DbExport.Services;

namespace Vertical.DbExport.PartitionedQuerying;

[ServiceRegistration(ServiceLifetime.Scoped)]
public class PartitionedQueryHandlerFactory
{
    private readonly ILogger<PartitionedQueryHandlerFactory> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly RestorePoint _restorePoint;
    private readonly IDataChannelFactory _dataChannelFactory;
    private readonly DataFileAggregator _dataFileAggregator;

    /// <summary>
    /// Creates a new instance of this type
    /// </summary>
    public PartitionedQueryHandlerFactory(
        ILoggerFactory loggerFactory, 
        RestorePoint restorePoint,
        IDataChannelFactory dataChannelFactory,
        DataFileAggregator dataFileAggregator)
    {
        _logger = loggerFactory.CreateLogger<PartitionedQueryHandlerFactory>();
        _loggerFactory = loggerFactory;
        _restorePoint = restorePoint;
        _dataChannelFactory = dataChannelFactory;
        _dataFileAggregator = dataFileAggregator;
    }

    public IPartitionedQueryHandler CreateHandler(PartitionedQuery partitionedQuery)
    {
        var schema = partitionedQuery.Job.DataSource.SchemaName;
        var table = partitionedQuery.Job.DataSource.TableName;
        var parquetChannel = _dataChannelFactory.CreateChannel(
            partitionedQuery.FileSystem, 
            $"{schema}.{table}_{partitionedQuery.PartitionId:0000000}");

        var logger = _loggerFactory.CreateLogger<PartitionedQueryHandler>();

        return new PartitionedQueryHandler(
            logger, 
            partitionedQuery, 
            parquetChannel, 
            _restorePoint,
            _dataFileAggregator);
    }
}