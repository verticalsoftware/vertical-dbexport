using Vertical.DbExport.Models;
using Vertical.DbExport.Options;

namespace Vertical.DbExport.Pipeline;

public class JobContext : ExportContext
{
    public JobOptions Job { get; set; } = default!;

    public string RestorePointSha { get; set; } = default!;

    public Guid Id { get; } = Guid.NewGuid();

    public IReadOnlyList<ColumnSchema> ColumnSchemata { get; set; } = default!;
    
    public ColumnSchema? SortColumnSchema { get; set; }
}