using Vertical.DbExport.Providers;

namespace Vertical.DbExport.Infrastructure;

public interface IDatabaseProviderFactory
{
    IDatabaseProvider? GetProvider(string providerId);
}

[Inject]
public class DatabaseProviderFactory : IDatabaseProviderFactory
{
    private readonly IEnumerable<IDatabaseProvider> _providers;

    public DatabaseProviderFactory(IEnumerable<IDatabaseProvider> providers)
    {
        _providers = providers;
    }

    public IDatabaseProvider? GetProvider(string providerId)
    {
        return _providers.FirstOrDefault(p => p.ProviderId.Equals(providerId, StringComparison.OrdinalIgnoreCase));
    }
}