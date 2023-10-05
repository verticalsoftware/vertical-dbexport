namespace Vertical.DbExport;

public class RootOptions
{
    public Dictionary<string, ConnectionOptions> Connections { get; set; } = new();
    public Dictionary<string, JobOptions> Jobs { get; set; } = new();
}