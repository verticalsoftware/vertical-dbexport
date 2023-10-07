using Vertical.DbExport.Options;

namespace Vertical.DbExport.Providers;

public interface IDatabaseProvider
{
    string ProviderId { get; }

    Task<bool> TestConnectionAsync(ConnectionOptions options);

    TaskBatchOperation CreateBatchingOperation(ConnectionOptions connection, TaskOptions task);
}