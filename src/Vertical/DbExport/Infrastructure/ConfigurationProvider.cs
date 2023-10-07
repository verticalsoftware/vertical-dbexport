using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Vertical.DbExport.Options;

namespace Vertical.DbExport.Infrastructure;

public interface IConfigurationProvider
{
    IConfiguration GetConfiguration();
}

[Inject]
public class ConfigurationProvider : IConfigurationProvider
{
    private readonly CommandLineOptions _commandLineOptions;
    private readonly ILogger<ConfigurationProvider> _logger;
    private readonly Lazy<IConfiguration> _lazyConfiguration;
    
    public ConfigurationProvider(CommandLineOptions commandLineOptions, ILogger<ConfigurationProvider> logger)
    {
        _commandLineOptions = commandLineOptions;
        _logger = logger;
        _lazyConfiguration = new Lazy<IConfiguration>(Build);
    }

    public IConfiguration GetConfiguration() => _lazyConfiguration.Value;

    private IConfiguration Build()
    {
        var builder = new ConfigurationBuilder();

        if (_commandLineOptions.ConfigFiles.Count == 0)
        {
            _logger.LogWarning("No configuration file(s) merged to root options.");
        }
        
        foreach (var source in _commandLineOptions.ConfigFiles.Select(Path.GetFullPath))
        {
            _logger.LogDebug("Adding configuration {path}", source);
            builder.AddJsonFile(source);
        }

        builder.AddUserSecrets(GetType().Assembly);

        return builder.Build();
    }
}