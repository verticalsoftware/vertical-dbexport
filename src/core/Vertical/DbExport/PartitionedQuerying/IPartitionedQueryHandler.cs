using Vertical.DbExport.Services;

namespace Vertical.DbExport.PartitionedQuerying;

public interface IPartitionedQueryHandler
{
    Task<long> ExecuteToChannelAsync(CancellationToken cancellationToken);
}