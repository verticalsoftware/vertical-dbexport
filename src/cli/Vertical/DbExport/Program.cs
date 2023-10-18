using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vertical.CommandLine;
using Vertical.DbExport;
using Vertical.DbExport.MySql;
using Vertical.DbExport.Options;
using Vertical.DbExport.Services;
using Vertical.SpectreLogger;
using Vertical.SpectreLogger.Options;
using Vertical.SpectreLogger.Rendering;

var parser = ProgramArguments.CreateConfiguration();
var arguments = CommandLineApplication.ParseArguments<ProgramArguments>(parser, args);

var services = new ServiceCollection()
    .AddCoreServices(builder => builder.AddMySql())
    .AddServicesFromAssembly(typeof(Program).Assembly)
    .AddLogging(builder =>
    {
        builder.AddSpectreConsole(logger => logger
            .ConfigureProfiles(profile =>
            {
                profile.OutputTemplate = profile.OutputTemplate!.Replace("{Message}", "{Scopes}{Message}");
                profile.ConfigureOptions<ExceptionRenderer.Options>(opt => opt.MaxStackFrames = 10);
            })
            .SetMinimumLevel(arguments.LoggingLevel));
        
        builder.SetMinimumLevel(arguments.LoggingLevel);
    })
    .BuildServiceProvider();

await Controller.RunAsync(services, args); 