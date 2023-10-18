using Vertical.DbExport.Models;

namespace Vertical.DbExport.Data;

public static class RecordExtensions
{
    public static T GetRequiredValue<T>(this Record record, string field)
    {
        return Cast<T>(record.GetNonNullValue(field), field);
    }
    
    public static TDest GetRequiredValue<TSrc, TDest>(this Record record, string field, Func<TSrc, TDest> converter)
    {
        return converter(Cast<TSrc>(record.GetNonNullValue(field), field));
    }

    public static T? GetValue<T>(this Record record, string field)
    {
        var obj = record.GetNullableValue(field);

        return obj == null ? default : Cast<T>(obj, field);
    }

    public static TDest? GetValue<TSrc, TDest>(
        this Record record,
        string field,
        Func<TSrc, TDest> converter,
        TDest? defaultValue = default)
    {
        var obj = record.GetNullableValue(field);

        return obj == null ? defaultValue : converter(Cast<TSrc>(obj, field));
    }

    private static T Cast<T>(object obj, string field)
    {
        try
        {
            return (T)obj;
        }
        catch (InvalidCastException exception)
        {
            throw new InvalidCastException($"{exception.Message} (field='{field}')");
        }
    }

    private static object GetNonNullValue(this Record record, string field)
    {
        var obj = record.Properties[field];

        if (obj is null or DBNull)
            throw new InvalidOperationException($"Expected non-null value for '{field}'.");

        return obj;
    }

    private static object? GetNullableValue(this Record record, string field)
    {
        var obj = record.Properties[field];

        return obj is DBNull ? null : obj;
    }
}