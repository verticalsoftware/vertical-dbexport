using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vertical.CommandLine;
using Vertical.DbExport.Exceptions;
using Vertical.DbExport.Options;
using Vertical.DbExport.Services;

namespace Vertical.DbExport;

[ServiceRegistration(ServiceLifetime.Singleton)]
public class Controller
{
    private readonly ILogger<Controller> _logger;
    private readonly OptionsLoader _optionsLoader;
    private readonly IExportController _exportController;

    public Controller(ILogger<Controller> logger, OptionsLoader optionsLoader, IExportController exportController)
    {
        _logger = logger;
        _optionsLoader = optionsLoader;
        _exportController = exportController;
    }

    public static async Task RunAsync(IServiceProvider services, IEnumerable<string> args)
    {
        var instance = services.GetRequiredService<Controller>();
        await instance.RunInternalAsync(args);
    }
    
    private async Task RunInternalAsync(IEnumerable<string> args)
    {
        try
        {
            var parser = ProgramArguments.CreateConfiguration();
            var arguments = CommandLineApplication.ParseArguments<ProgramArguments>(parser, args);
            var exportDefinitions = _optionsLoader.LoadOptions(arguments);
            var (current, count) = (1, exportDefinitions.Count);

            foreach (var exportDefinition in exportDefinitions)
            {
                _logger.LogInformation("Starting export {current} of {count}", current, count);
                await RunExportAsync(exportDefinition);
                _logger.LogInformation("Export complete");
                current++;
            }
        }
        catch (Exception exception)
        {
            _logger.LogCritical(exception, "Export stopped.");
        }
    }

    private async Task RunExportAsync(ExportOptions exportDefinition)
    {
        try
        {
            LogMetadata(exportDefinition);
            await _exportController.ExecuteAsync(exportDefinition, CancellationToken.None);
        }
        catch (CriticalStopException exception)
        {
            _logger.LogError("{message}", exception.Message);
        }
    }

    private void LogMetadata(ExportOptions exportDefinition)
    {
        if (exportDefinition is not { Metadata.Count: not 0 }) 
            return;
        
        foreach (var (key, value) in exportDefinition.Metadata)
        {
            _logger.LogInformation("\t{key}: {value}", key, value);
        }
    }
}