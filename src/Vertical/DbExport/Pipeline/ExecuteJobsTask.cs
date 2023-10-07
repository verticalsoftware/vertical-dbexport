using Microsoft.Extensions.Logging;
using Vertical.DbExport.Options;
using Vertical.DbExport.Services;
using Vertical.Pipelines;

namespace Vertical.DbExport.Pipeline;

public class ExecuteJobsTask : IPipelineMiddleware<RootOptions>
{
    private readonly ILogger<ExecuteJobsTask> _logger;
    private readonly IJobTaskService _jobTaskService;

    public ExecuteJobsTask(ILogger<ExecuteJobsTask> logger, IJobTaskService jobTaskService)
    {
        _logger = logger;
        _jobTaskService = jobTaskService;
    }
    
    public async Task InvokeAsync(RootOptions context, PipelineDelegate<RootOptions> next,
        CancellationToken cancellationToken)
    {
        try
        {
            if (context.Jobs.Length == 0)
            {
                _logger.LogWarning("No jobs have been defined in the runtime configuration.");
                return;
            }

            foreach (var job in context.Jobs)
            {
                _logger.LogInformation("Starting job {job}", job.JobId);
                
                foreach (var task in job.Tasks)
                {
                    await _jobTaskService.ExecuteTaskAsync(context, job, task, cancellationToken);
                }
                
                _logger.LogInformation("Completed job {job}", job.JobId);
            }
        }
        finally
        {
            await next(context, cancellationToken);
        }
    }
}