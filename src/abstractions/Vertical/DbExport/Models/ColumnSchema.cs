namespace Vertical.DbExport.Models;

/// <summary>
/// Defines a column schema.
/// </summary>
public class ColumnSchema
{
    /// <summary>
    /// Gets the column name.
    /// </summary>
    public required string ColumnName { get; init; } = default!;
    
    /// <summary>
    /// Gets the ordinal position.
    /// </summary>
    public required int OrdinalPosition { get; init; }

    /// <summary>
    /// Gets the data type.
    /// </summary>
    public required Type DataType { get; init; } = default!;
    
    /// <summary>
    /// Gets the max length if appropriate.
    /// </summary>
    public required int? MaxLength { get; init; }
    
    /// <summary>
    /// Gets whether the column is nullable.
    /// </summary>
    public required bool IsNullable { get; init; }
    
    /// <summary>
    /// Gets the column key type.
    /// </summary>
    public required ColumnKeyType KeyType { get; init; }

    /// <inheritdoc />
    public override string ToString() => $"{ColumnName} {DataType}/{(IsNullable ? "NULL" : "NOT NULL")}";
}