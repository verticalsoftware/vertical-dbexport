using Microsoft.Extensions.Logging;
using Vertical.DbExport.Infrastructure;
using Vertical.DbExport.Options;
using Vertical.DbExport.Utilities;

namespace Vertical.DbExport.Services;

public interface IJobTaskService
{
    Task<bool> ExecuteTaskAsync(
        RootOptions context,
        JobOptions job, 
        TaskOptions task, 
        CancellationToken cancellationToken);
}

[Inject]
public class JobTaskService : IJobTaskService
{
    private readonly ILogger<JobTaskService> _logger;
    private readonly IDatabaseProviderFactory _databaseProviderFactory;

    public JobTaskService(ILogger<JobTaskService> logger, IDatabaseProviderFactory databaseProviderFactory)
    {
        _logger = logger;
        _databaseProviderFactory = databaseProviderFactory;
    }

    public async Task<bool> ExecuteTaskAsync(
        RootOptions context,
        JobOptions job, 
        TaskOptions task, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting task {job}/{task}", job.JobId, task.TaskId);

        var connection = context.Connections[task.Connection];
        var dbProvider = _databaseProviderFactory.GetProvider(connection.ProviderId)
                         ?? throw new InvalidOperationException();
        var outputChannel = new OutputChannel(task.Output);
        var batchingOperation = dbProvider.CreateBatchingOperation(connection, task);

        while (batchingOperation.Ready)
        {
            var data = await batchingOperation.QueryAsync(cancellationToken);
            await outputChannel.WriteBatchAsync(data, cancellationToken);
        }

        await outputChannel.FinalizeAsync(cancellationToken);

        return true;
    }
}