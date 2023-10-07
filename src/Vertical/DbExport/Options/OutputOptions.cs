namespace Vertical.DbExport.Options;

public class OutputOptions
{
    public string Format { get; set; } = "json";
    public string Compression { get; set; } = "gzip";
    public string PathTemplate { get; set; } = "{$table}.{$n}.json.gz";
    public string? MaxUncompressedSize { get; set; }
}