using Microsoft.Extensions.Logging;
using Vertical.Pipelines;

namespace Vertical.DbExport.Pipeline.Jobs;

public class QuerySchemaTask : IPipelineMiddleware<JobContext>
{
    private readonly ILogger<QuerySchemaTask> _logger;

    /// <summary>
    /// Creates a new instance of this type
    /// </summary>
    public QuerySchemaTask(ILogger<QuerySchemaTask> logger)
    {
        _logger = logger;
    }
    
    /// <inheritdoc />
    public async Task InvokeAsync(
        JobContext context,
        PipelineDelegate<JobContext> next,
        CancellationToken cancellationToken)
    {
        var datasource = context.Job.DataSource;
        context.ColumnSchemata = await context
            .DatabaseProvider
            .CreateContext(datasource)
            .GetColumnSchemaAsync(cancellationToken);
        
        _logger.LogInformation("Column schema retrieved, selected column count = {count}", context.ColumnSchemata.Count);

        await next(context, cancellationToken);
    }
}