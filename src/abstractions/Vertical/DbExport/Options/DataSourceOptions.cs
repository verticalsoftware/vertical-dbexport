namespace Vertical.DbExport.Options;

/// <summary>
/// Represents options for the data source.
/// </summary>
public class DataSourceOptions
{
    /// <summary>
    /// Gets the schema.
    /// </summary>
    public string SchemaName { get; set; } = default!;
    
    /// <summary>
    /// Gets the table name.
    /// </summary>
    public string TableName { get; set; } = default!;
    
    /// <summary>
    /// Gets the column that the table is sorted on.
    /// </summary>
    public ColumnDefinition? SortColumn { get; set; }
    
    /// <summary>
    /// Gets the columns to include in the extract. If null or empty, all columns are selected.
    /// </summary>
    public string[]? SelectColumns { get; set; }
    
    /// <summary>
    /// Gets the columns to exclude from the extract.
    /// </summary>
    public string[]? ExcludeColumns { get; set; }
}