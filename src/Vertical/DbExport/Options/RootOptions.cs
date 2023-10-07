namespace Vertical.DbExport.Options;

public class RootOptions : RootOptionsBase
{
    public Dictionary<string, ConnectionOptions> Connections { get; set; } = new();
    public JobOptions[] Jobs { get; set; } = Array.Empty<JobOptions>();
}