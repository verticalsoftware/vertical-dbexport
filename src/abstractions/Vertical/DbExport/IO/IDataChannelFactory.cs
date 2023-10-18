using Vertical.DbExport.Services;

namespace Vertical.DbExport.IO;

public interface IDataChannelFactory
{
    IDataChannel CreateChannel(IFileSystem fileSystem, string path);
}