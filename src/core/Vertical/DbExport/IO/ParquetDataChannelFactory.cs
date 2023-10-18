using Microsoft.Extensions.DependencyInjection;
using Vertical.DbExport.Services;

namespace Vertical.DbExport.IO;

[ServiceRegistration(ServiceLifetime.Singleton)]
public class ParquetDataChannelFactory : IDataChannelFactory
{
    /// <inheritdoc />
    public IDataChannel CreateChannel(IFileSystem fileSystem, string path)
    {
        return new ParquetDataChannel(fileSystem, path);
    }
}