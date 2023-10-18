namespace Vertical.DbExport.Models;

public readonly record struct Record(IDictionary<string, object?> Properties)
{
    public object? this[string key] => Properties[key];
}