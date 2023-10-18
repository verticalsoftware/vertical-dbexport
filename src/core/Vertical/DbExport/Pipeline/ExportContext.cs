using Vertical.DbExport.Options;
using Vertical.DbExport.Services;

namespace Vertical.DbExport.Pipeline;

public class ExportContext
{
    public required ExportOptions Options { get; init; }

    public IDatabaseProvider DatabaseProvider { get; set; } = default!;
}