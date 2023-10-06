namespace Vertical.DbExport;

public interface IConnectionProviderFactory
{
    IConnectionProvider? GetProvider(string providerId);
}

[Inject]
public class ConnectionProviderFactory : IConnectionProviderFactory
{
    private readonly IEnumerable<IConnectionProvider> _providers;

    public ConnectionProviderFactory(IEnumerable<IConnectionProvider> providers)
    {
        _providers = providers;
    }

    public IConnectionProvider? GetProvider(string providerId)
    {
        return _providers.FirstOrDefault(p => p.ProviderId.Equals(providerId, StringComparison.OrdinalIgnoreCase));
    }
}