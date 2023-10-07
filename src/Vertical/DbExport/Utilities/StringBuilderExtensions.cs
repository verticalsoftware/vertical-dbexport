using System.Text;

namespace Vertical.DbExport.Utilities;

public static class StringBuilderExtensions
{
    public static StringBuilder AppendCsv<T>(this StringBuilder builder, IEnumerable<T> items, string separator = ",")
    {
        var c = string.Empty;

        foreach (var item in items)
        {
            builder.Append(c);
            builder.Append(item);
            c = separator;
        }

        return builder;
    }

    public static StringBuilder AppendCsvLine<T>(this StringBuilder builder, IEnumerable<T> items,
        string separator = ",")
    {
        builder.AppendCsv(items, separator);
        builder.AppendLine();
        return builder;
    }
}