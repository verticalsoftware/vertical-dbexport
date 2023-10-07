using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vertical.DbExport.Options;
using Vertical.DbExport.Pipeline;
using Vertical.Pipelines.DependencyInjection;
using Vertical.SpectreLogger;
using Vertical.SpectreLogger.Options;

namespace Vertical.DbExport.Infrastructure;

public static class Runtime
{
    public static async Task RunAsync(CommandLineOptions options)
    {
        var services = new ServiceCollection();

        services.AddSingleton(options);
        services.AddApplicationServices();
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.ConfigurePipeline<RootOptions>(builder => builder
            .UseMiddleware<ValidateConfigurationTask>()
            .UseMiddleware<ExecuteJobsTask>()
        );
        services.AddLogging(builder =>
        {
            builder.AddSpectreConsole(cfg =>
            {
                cfg.UseSerilogConsoleStyle();
                cfg.SetMinimumLevel(options.LogLevel);
            });
            builder.SetMinimumLevel(options.LogLevel);
        });

        await using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<PipelineRunner>().ExecuteAsync();
    }
}