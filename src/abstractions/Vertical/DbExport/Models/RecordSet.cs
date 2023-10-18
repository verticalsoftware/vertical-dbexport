namespace Vertical.DbExport.Models;

public readonly record struct RecordSet(
    IReadOnlyList<ColumnSchema> Columns,
    IReadOnlyList<Record> Rows);