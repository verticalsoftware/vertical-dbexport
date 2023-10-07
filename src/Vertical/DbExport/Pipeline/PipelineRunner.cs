using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Vertical.DbExport.Infrastructure;
using Vertical.DbExport.Options;
using Vertical.Pipelines;
using IConfigurationProvider = Vertical.DbExport.Infrastructure.IConfigurationProvider;

namespace Vertical.DbExport.Pipeline;

[Inject(ServiceLifetime.Scoped)]
public class PipelineRunner
{
    private readonly IPipelineFactory<RootOptions> _pipelineFactory;
    private readonly IConfigurationProvider _configurationProvider;

    public PipelineRunner(IPipelineFactory<RootOptions> pipelineFactory,
        IConfigurationProvider configurationProvider)
    {
        _pipelineFactory = pipelineFactory;
        _configurationProvider = configurationProvider;
    }

    public async Task ExecuteAsync()
    {
        var configuration = _configurationProvider.GetConfiguration();
        var options = new RootOptions();
        configuration.Bind(options);

        var pipeline = _pipelineFactory.CreatePipeline();
        await pipeline(new RootOptions(), CancellationToken.None);
    }
}