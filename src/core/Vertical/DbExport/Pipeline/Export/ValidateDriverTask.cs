using Microsoft.Extensions.Logging;
using Vertical.DbExport.Services;
using Vertical.Pipelines;

namespace Vertical.DbExport.Pipeline.Export;

public class ValidateDriverTask : IPipelineMiddleware<ExportContext>
{
    private readonly ILogger<ValidateDriverTask> _logger;
    private readonly IDatabaseProviderFactory _databaseProviderFactory;

    /// <summary>
    /// Creates a new instance of this type
    /// </summary>
    public ValidateDriverTask(
        ILogger<ValidateDriverTask> logger,
        IDatabaseProviderFactory databaseProviderFactory)
    {
        _logger = logger;
        _databaseProviderFactory = databaseProviderFactory;
    }   
    
    /// <inheritdoc />
    public async Task InvokeAsync(
        ExportContext context,
        PipelineDelegate<ExportContext> next,
        CancellationToken cancellationToken)
    {
        var driver = context.Options.Connection.Driver;
        var provider = _databaseProviderFactory.GetProvider(context.Options.Connection);
        provider.ConnectionOptions = context.Options.Connection;
        
        await provider.ValidateAsync(cancellationToken);
        _logger.LogInformation("Validated {driver} database driver.", driver);

        context.DatabaseProvider = provider;
        
        await next(context, cancellationToken);
    }
}