using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vertical.DbExport.Serialization;
using Vertical.DbExport.Services;

namespace Vertical.DbExport.Options;

[ServiceRegistration(ServiceLifetime.Singleton)]
public class OptionsLoader
{
    private readonly ILogger<OptionsLoader> _logger;

    public OptionsLoader(ILogger<OptionsLoader> logger)
    {
        _logger = logger;
    }

    public IReadOnlyList<ExportOptions> LoadOptions(ProgramArguments programArguments)
    {
        return programArguments
            .DefinitionFiles
            .Select(LoadOptionsFile)
            .ToArray();
    }

    private ExportOptions LoadOptionsFile(string path)
    {
        _logger.LogInformation("Loading export definition {path}", path);

        using var stream = File.OpenRead(path);
        var document = JsonSerializer.Deserialize<ExportOptions>(stream, JsonOptions.Compact);
        return document ??
               throw new ApplicationException("Failed to deserialize file content to export definition");
    }
}