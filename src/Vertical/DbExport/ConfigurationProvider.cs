using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Vertical.DbExport;

public interface IConfigurationProvider
{
    IConfiguration GetConfiguration();
}

[Inject]
public class ConfigurationProvider : IConfigurationProvider
{
    private readonly CommandLineOptions _options;
    private readonly ILogger<ConfigurationProvider> _logger;
    private readonly Lazy<IConfiguration> _lazyConfiguration;
    
    public ConfigurationProvider(CommandLineOptions options, ILogger<ConfigurationProvider> logger)
    {
        _options = options;
        _logger = logger;
        _lazyConfiguration = new Lazy<IConfiguration>(Build);
    }

    public IConfiguration GetConfiguration() => _lazyConfiguration.Value;

    private IConfiguration Build()
    {
        var builder = new ConfigurationBuilder();
        var parameters = ParameterDictionary.Create(_options.Parameters);
        foreach (var source in _options.ConfigFiles.Select(path => TokenResolver.Resolve(path, parameters)))
        {
            _logger.LogDebug("Adding configuration {path}", source);
            builder.AddJsonFile(source);
        }

        return builder.Build();
    }
}