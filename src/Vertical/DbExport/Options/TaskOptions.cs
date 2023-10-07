namespace Vertical.DbExport.Options;

public class TaskOptions
{
    public string TaskId { get; set; } = Guid.NewGuid().ToString();
    public string Connection { get; set; } = default!;
    public string? Schema { get; set; }
    public string? Table { get; set; }
    public string? Query { get; set; }
    public string[]? Select { get; set; }
    public string[]? Sort { get; set; }
    public int? BatchCount { get; set; }
    public int? CommandRetries { get; set; }
    public int? RecordLimit { get; set; }
    public int? BatchLimit { get; set; }
    public required OutputOptions Output { get; set; } = default!;
}