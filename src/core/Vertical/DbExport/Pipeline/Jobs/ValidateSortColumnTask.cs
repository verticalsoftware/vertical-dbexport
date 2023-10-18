using Microsoft.Extensions.Logging;
using Vertical.DbExport.Exceptions;
using Vertical.DbExport.Models;
using Vertical.Pipelines;

namespace Vertical.DbExport.Pipeline.Jobs;

public class ValidateSortColumnTask : IPipelineMiddleware<JobContext>
{
    private readonly ILogger<ValidateSortColumnTask> _logger;

    /// <summary>
    /// Creates a new instance of this type
    /// </summary>
    public ValidateSortColumnTask(ILogger<ValidateSortColumnTask> logger)
    {
        _logger = logger;
    }
    
    /// <inheritdoc />
    public async Task InvokeAsync(
        JobContext context,
        PipelineDelegate<JobContext> next,
        CancellationToken cancellationToken)
    {
        var sortColumn = context.Job.DataSource.SortColumn;
        if (sortColumn == null)
        {
            await next(context, cancellationToken);
            return;
        }

        var matchedColumnSchema = context.ColumnSchemata.FirstOrDefault(column => 
            column.ColumnName == sortColumn.ColumnName);

        if (matchedColumnSchema == null)
        {
            _logger.LogError("Specified sort column '{sortColumn}' was not found in the schema.",
                sortColumn.ColumnName);
            throw new CriticalStopException();
        }

        if (matchedColumnSchema.KeyType == ColumnKeyType.None)
        {
            _logger.LogWarning("Specified sort column '{sortColumn}' is not indexed, performance may be affected.",
                sortColumn.ColumnName);
        }
        
        _logger.LogInformation("Validated sort column schema '{sortColumn}' (index={index})",
            sortColumn.ColumnName,
            matchedColumnSchema.KeyType);

        context.SortColumnSchema = matchedColumnSchema;

        await next(context, cancellationToken);
    }
}