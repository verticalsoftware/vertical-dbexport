using Microsoft.Extensions.Logging;
using Vertical.CommandLine.Configuration;

namespace Vertical.DbExport.Options;

/// <summary>
/// Defines program arguments.
/// </summary>
public class ProgramArguments
{
    /// <summary>
    /// Gets the log level.
    /// </summary>
    public LogLevel LoggingLevel { get; set; }

    /// <summary>
    /// Gets the definition files.
    /// </summary>
    public List<string> DefinitionFiles { get; } = new();

    public static ApplicationConfiguration<ProgramArguments> CreateConfiguration()
    {
        var config = new ApplicationConfiguration<ProgramArguments>();

        config
            .Option<LogLevel>("--log-level", arg => arg.Map.ToProperty(opt => opt.LoggingLevel))
            .PositionArgument(arg => arg.MapMany.ToCollection(opt => opt.DefinitionFiles));
        
        return config;
    }
}