namespace Vertical.DbExport;

public interface IConnectionProvider
{
    string ProviderId { get; }

    Task<bool> TestAsync(ConnectionOptions options);
}