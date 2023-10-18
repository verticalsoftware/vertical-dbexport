using Vertical.DbExport.Data;
using Vertical.DbExport.Models;

namespace Vertical.DbExport.MySql;

internal static class MySqlUtilities
{
    internal static bool IsNullableSchemaField(this Record record)
    {
        return record.GetRequiredValue<string>("IS_NULLABLE") == "YES";
    }

    internal static Type GetSchemaFieldType(this Record record)
    {
        var isNullable = record.IsNullableSchemaField();
        var metadata = record.GetRequiredValue<string>("DATA_TYPE");
        var baseType = GetSchemaFieldBaseType(metadata);

        return !baseType.IsClass && isNullable
            ? typeof(Nullable<>).MakeGenericType(baseType)
            : baseType;
    }
    
    internal static ColumnKeyType GetSchemaKeyType(this Record record)
    {
        var keyType = record.GetRequiredValue<string>("COLUMN_KEY");
        
        return keyType switch
        {
            "PRI" => ColumnKeyType.PrimaryKey,
            "UNI" => ColumnKeyType.Unique,
            _ => ColumnKeyType.None
        };
    }
    
    private static Type GetSchemaFieldBaseType(string value)
    {
        if (value.StartsWith("char"))
            return typeof(string);
        if (value.StartsWith("varchar"))
            return typeof(string);
        if (value.StartsWith("json"))
            return typeof(string);
        if (value.StartsWith("tinyint"))
            return typeof(sbyte);
        if (value.StartsWith("decimal"))
            return typeof(decimal);
        if (value.StartsWith("numeric"))
            return typeof(decimal);
        if (value.StartsWith("smallint"))
            return typeof(int);
        if (value.StartsWith("mediumint"))
            return typeof(int);
        if (value.StartsWith("int"))
            return typeof(int);
        if (value.StartsWith("integer"))
            return typeof(int);
        if (value.StartsWith("bigint"))
            return typeof(long);
        
        return value switch
        {
            "datetime" => typeof(DateTime),
            "date" => typeof(DateTime),
            "timestamp" => typeof(DateTime),
            "time" => typeof(DateTime),
            "enum" => typeof(string),
            "blob" => typeof(byte[]),
            _ => throw new InvalidOperationException($"Unsupported MySql data type {value}")
        };
    }
}