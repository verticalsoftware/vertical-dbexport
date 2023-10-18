using Vertical.DbExport.Services;
using Vertical.Pipelines;

namespace Vertical.DbExport.Pipeline.Jobs;

public class FlushDataFilesTask : IPipelineMiddleware<JobContext>
{
    private readonly DataFileAggregator _dataFileAggregator;

    public FlushDataFilesTask(DataFileAggregator dataFileAggregator)
    {
        _dataFileAggregator = dataFileAggregator;
    }

    /// <inheritdoc />
    public async Task InvokeAsync(
        JobContext context,
        PipelineDelegate<JobContext> next,
        CancellationToken cancellationToken)
    {
        await _dataFileAggregator.FlushAsync();
        
        await next(context, cancellationToken);
    }
}