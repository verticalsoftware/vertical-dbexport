using System.Text.Json;
using System.Text.Json.Serialization;

namespace Vertical.DbExport.Serialization;

public static class JsonOptions
{
    public static JsonSerializerOptions Compact { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    public static JsonSerializerOptions Friendly { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };
}