namespace Vertical.DbExport;

public class JobOptions
{
    public Dictionary<string, TaskOptions> Tasks { get; set; } = new();
}