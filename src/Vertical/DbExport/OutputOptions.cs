namespace Vertical.DbExport;

public class OutputOptions
{
    public string Format { get; set; } = "json";
    public string Compression { get; set; } = "gzip";
    public string NamingConvention { get; set; } = "{$table}.{$n}.json.gz";
}