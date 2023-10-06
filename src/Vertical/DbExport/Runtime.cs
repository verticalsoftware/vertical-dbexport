using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vertical.SpectreLogger;
using Vertical.SpectreLogger.Options;

namespace Vertical.DbExport;

public static class Runtime
{
    public static async Task RunAsync(CommandLineOptions options)
    {
        var services = new ServiceCollection();

        services.AddSingleton(options);
        services.AddApplicationServices();
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
    }
}