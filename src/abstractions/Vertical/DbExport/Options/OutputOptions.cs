using System.Text.RegularExpressions;

namespace Vertical.DbExport.Options;

/// <summary>
/// Gets the output options.
/// </summary>
public partial class OutputOptions
{
    public static readonly Regex FileTemplateExpression = MyRegex();
    
    /// <summary>
    /// Gets the desired size of the output file. The loader will attempts to get as
    /// close to this as possible.
    /// </summary>
    public string FileSize { get; set; } = "25mb";
    
    /// <summary>
    /// Gets the optional path for final output.
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// Gets the output file template.
    /// </summary>
    public string FileNameTemplate { get; set; } = "$(table).$(sequence)";
    
    /// <summary>
    /// Gets the output type.
    /// </summary>
    public OutputFormat Format { get; set; }
    
    /// <summary>
    /// Gets the output file compression.
    /// </summary>
    public OutputFileCompression Compression { get; set; }

    /// <summary>
    /// Gets the size specification in bytes.
    /// </summary>
    /// <returns></returns>
    public long GetFileSizeInBytes()
    {
        var match = FileTemplateExpression.Match(FileSize);
        var digits = int.Parse(match.Groups[1].Value);
        return match.Groups[2].Value switch
        {
            "kb" => digits * 1_000,
            "mb" => digits * 1_000_000,
            _ => digits * 1_000_000_000
        };
    }

    [GeneratedRegex("(\\d+)([kmg]b)")]
    private static partial Regex MyRegex();
}