using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Vertical.DbExport.Exceptions;
using Vertical.DbExport.Options;

namespace Vertical.DbExport.Services;

[ServiceRegistration(ServiceLifetime.Singleton)]
public class DatabaseProviderFactory : IDatabaseProviderFactory
{
    private readonly IEnumerable<IDatabaseProvider> _providers;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<DatabaseProviderFactory> _logger;

    /// <summary>
    /// Creates a new instance of this type
    /// </summary>
    public DatabaseProviderFactory(IEnumerable<IDatabaseProvider> providers, 
        ILoggerFactory loggerFactory)
    {
        _providers = providers;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<DatabaseProviderFactory>();
    }
    
    /// <inheritdoc />
    public IDatabaseProvider GetProvider(ConnectionOptions connectionOptions)
    {
        _logger.LogTrace("Resolving database provider (driver={driver})", connectionOptions.Driver);

        var provider = _providers.FirstOrDefault(provider => provider.DriverId == connectionOptions.Driver);
        if (provider == null)
        {
            _logger.LogError("Could not resolve database provider '{driver}'", connectionOptions.Driver);
            throw new CriticalStopException();
        }

        return provider;
    }
}