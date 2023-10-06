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


        await next(context, cancellationToken);
    }
}