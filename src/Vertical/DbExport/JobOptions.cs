namespace Vertical.DbExport;

public class JobOptions
{
    public string JobId { get; set; } = default!;
    public TaskOptions[] Tasks { get; set; } = Array.Empty<TaskOptions>();
    public Dictionary<string, string>? Parameters { get; set; }
}