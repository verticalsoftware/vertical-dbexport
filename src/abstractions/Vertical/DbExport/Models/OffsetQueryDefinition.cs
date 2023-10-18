namespace Vertical.DbExport.Models;

public record OffsetQueryDefinition(
    string Sql, 
    IReadOnlyList<ColumnSchema> ColumnSchemata, 
    ColumnSchema? SortColumn = null);