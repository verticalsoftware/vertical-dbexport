namespace Vertical.DbExport;

public class TaskOptions
{
    public string TaskId { get; set; } = Guid.NewGuid().ToString();
    public string Connection { get; set; } = default!;
    public string? Table { get; set; }
    public string? Query { get; set; }
    public string[]? Columns { get; set; }
    public int? BatchCount { get; set; }
    public required OutputOptions Output { get; set; } = default!;
    public Dictionary<string, string>? Parameters { get; set; }
}