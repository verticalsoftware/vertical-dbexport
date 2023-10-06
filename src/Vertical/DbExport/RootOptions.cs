namespace Vertical.DbExport;

public class RootOptions
{
    public Dictionary<string, ConnectionOptions> Connections { get; set; } = new();
    public JobOptions[] Jobs { get; set; } = Array.Empty<JobOptions>();
    public Dictionary<string, string>? Parameters { get; set; }
}