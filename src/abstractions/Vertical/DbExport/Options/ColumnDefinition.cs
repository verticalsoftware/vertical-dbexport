namespace Vertical.DbExport.Options;

/// <summary>
/// Represents a column specification.
/// </summary>
public class ColumnDefinition
{
    /// <summary>
    /// Gets the column name.
    /// </summary>
    public string ColumnName { get; set; } = default!;
    
    /// <summary>
    /// Gets the default value.
    /// </summary>
    public string? DefaultValue { get; set; }
}