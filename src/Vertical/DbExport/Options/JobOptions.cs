namespace Vertical.DbExport.Options;

public class JobOptions
{
    public string JobId { get; set; } = Guid.NewGuid().ToString();
    public TaskOptions[] Tasks { get; set; } = Array.Empty<TaskOptions>();
}