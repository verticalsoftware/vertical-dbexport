using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vertical.DbExport.Extensions;
using Vertical.DbExport.Options;
using Vertical.DbExport.Pipeline;
using Vertical.Pipelines;

namespace Vertical.DbExport.Services;

/// <summary>
/// Orchestrates the export.
/// </summary>
[ServiceRegistration(ServiceLifetime.Singleton)]
public class ExportController : IExportController
{
    private readonly ILogger<ExportController> _logger;
    private readonly IPipelineFactory<ExportContext> _pipelineFactory;

    /// <summary>
    /// Creates a new instance of this type
    /// </summary>
    public ExportController(
        ILogger<ExportController> logger,
        IPipelineFactory<ExportContext> pipelineFactory)
    {
        _logger = logger;
        _pipelineFactory = pipelineFactory;
    }
    
    /// <inheritdoc />
    public async Task ExecuteAsync(ExportOptions options, CancellationToken cancellationToken)
    {
        var pipeline = _pipelineFactory.CreatePipeline();
        
        await pipeline(new ExportContext
        {
            Options = options
        }, cancellationToken);
    }
}