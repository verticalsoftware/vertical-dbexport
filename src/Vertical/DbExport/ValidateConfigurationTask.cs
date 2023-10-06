using Microsoft.Extensions.Logging;
using Spectre.Console;
using Vertical.Pipelines;

namespace Vertical.DbExport;

public class ValidateConfigurationTask : IPipelineMiddleware<RootOptions>
{
    private readonly ILogger<ValidateConfigurationTask> _logger;
    private readonly IConnectionProviderFactory _connectionProviderFactory;

    public ValidateConfigurationTask(
        ILogger<ValidateConfigurationTask> logger,
        IConnectionProviderFactory connectionProviderFactory)
    {
        _logger = logger;
        _connectionProviderFactory = connectionProviderFactory;
    }

    public async Task InvokeAsync(
        RootOptions context,
        PipelineDelegate<RootOptions> next,
        CancellationToken cancellationToken)
    {
        var errors = await ValidateConnectionsAsync(context.Connections);

        if (errors > 0)
            return;

        await next(context, cancellationToken);
    }

    private int ValidateJobs(RootOptions options)
    {
        var errors = 0;
        var tasks = 0;
        var j = 0;
        
        foreach (var job in options.Jobs)
        {
            if (string.IsNullOrWhiteSpace(job.JobId))
            {
                errors++;
                _logger.LogError("Job[{index}]: empty job id.", j);
            }

            foreach (var task in job.Tasks)
            {
                if (string.IsNullOrWhiteSpace(task.TaskId))
                {
                    errors++;
                    _logger.LogError("Job[{job}].Tasks[{id}]: empty task id.", j, tasks);
                }

                if (string.IsNullOrWhiteSpace(task.Connection))
                {
                    errors++;
                    _logger.LogError("Job['{job}'].Tasks['{id}']: empty connection key.", job.JobId, task.TaskId);
                }

                if (string.IsNullOrWhiteSpace(task.Table) && string.IsNullOrWhiteSpace(task.Query))
                {
                    errors++;
                    _logger.LogError("Job['{job}'].Tasks['{id}']: unable to determine data source (Table and Query both empty).", 
                        job.JobId, task.TaskId);
                }

                if (task.BatchCount is < 1)
                {
                    errors++;
                    _logger.LogError("Job['{job}'].Tasks['{id}']: batch count is less than one.", 
                        job.JobId, task.TaskId);
                }

                tasks++;
            }

            j++;
        }

        if (tasks == 0)
        {
            errors++;
            
        }
    }

    private async Task<int> ValidateConnectionsAsync(Dictionary<string, ConnectionOptions> connections)
    {
        var errors = 0;
        
        if (connections.Count == 0)
        {
            errors++;
            _logger.LogError("No connections configured");
        }

        foreach (var (key, connectionOptions) in connections)
        {
            if (string.IsNullOrWhiteSpace(connectionOptions.ProviderId))
            {
                _logger.LogError("Connection {id}: empty provider id.", key);
                errors++;
                break;
            }

            var provider = _connectionProviderFactory.GetProvider(connectionOptions.ProviderId);
            if (provider == null)
            {
                _logger.LogError("Connection {id}: invalid provider id '{provider}'.", key, connectionOptions.ProviderId);
                errors++;
                break;
            }

            if (await provider.TestAsync(connectionOptions)) 
                continue;
            
            _logger.LogError("Connection {id}: failed provider '{provider}' connection test",
                key, connectionOptions.ProviderId);
            
            errors++;
            break;
        }

        return errors;
    }
}