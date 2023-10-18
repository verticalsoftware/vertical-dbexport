using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vertical.DbExport.Extensions;
using Vertical.DbExport.Options;
using Vertical.Pipelines;

namespace Vertical.DbExport.Pipeline.Export;

public class OrchestrateJobsTask : IPipelineMiddleware<ExportContext>
{
    private readonly ILogger<OrchestrateJobsTask> _logger;
    private readonly IServiceProvider _serviceProvider;

    public OrchestrateJobsTask(ILogger<OrchestrateJobsTask> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }
    
    /// <inheritdoc />
    public async Task InvokeAsync(
        ExportContext context,
        PipelineDelegate<ExportContext> next,
        CancellationToken cancellationToken)
    {
        foreach (var job in context.Options.Jobs)
        {
            await ExecuteJobAsync(context, job, cancellationToken);
        }
    }

    private async Task ExecuteJobAsync(
        ExportContext context, 
        JobOptions job,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting job {name}.", job.Name);

        await using var scope = _serviceProvider.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var pipelineFactory = services.GetRequiredService<IPipelineFactory<JobContext>>();
        var pipeline = pipelineFactory.CreatePipeline();
        var jobContext = new JobContext
        {
            Options = context.Options,
            DatabaseProvider = context.DatabaseProvider,
            Job = job,
            RestorePointSha = job.Sha()
        };

        await pipeline(jobContext, cancellationToken);
    }
}