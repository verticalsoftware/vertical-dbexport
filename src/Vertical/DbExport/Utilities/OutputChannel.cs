using Vertical.DbExport.Options;

namespace Vertical.DbExport.Utilities;

public class OutputChannel
{
    public OutputChannel(OutputOptions options)
    {
        
    }

    public async Task WriteBatchAsync(IEnumerable<object> data, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }

    public async Task FinalizeAsync(CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
    }
}