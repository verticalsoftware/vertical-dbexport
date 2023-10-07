namespace Vertical.DbExport.Providers;

public abstract class TaskBatchOperation : IAsyncDisposable
{
    public bool Ready { get; protected set; } = true;

    public abstract Task<IEnumerable<object>> QueryAsync(CancellationToken cancellationToken);

    public abstract ValueTask DisposeAsync();
}