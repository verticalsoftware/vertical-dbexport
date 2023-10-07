using Microsoft.Extensions.Logging;
using Vertical.CommandLine;
using Vertical.CommandLine.Configuration;

namespace Vertical.DbExport.Options;

public class CommandLineOptions
{
    public Dictionary<string, string> Parameters { get; } = new();
    public HashSet<string> ConfigFiles { get; } = new();
    public HashSet<string> Jobs { get; } = new();
    public HashSet<string> Tasks { get; } = new();
    public LogLevel LogLevel { get; set; } = LogLevel.Information;

    public static ApplicationConfiguration<CommandLineOptions> CreateConfiguration()
    {
        var app = new ApplicationConfiguration<CommandLineOptions>();

        app.Option("-t|--task", arg => arg.MapMany.ToSet(opt => opt.Tasks));
        app.Option("-j|--job", arg => arg.MapMany.ToSet(opt => opt.Jobs));
        app.Option("-c|--config", arg => arg.MapMany.ToSet(opt => opt.ConfigFiles));
        app.Option("--param", arg => arg.MapMany.Using((opt, value) =>
        {
            var split = value.Split('=').Select(a => a.Trim()).ToArray();
            if (split.Length != 2) throw new UsageException("Invalid parameter argument (format is key=<value>)");
            opt.Parameters[split[0]] = split[1];
        }));
        app.Option<LogLevel>("--log-level", arg => arg.Map.ToProperty(opt => opt.LogLevel));
        
        return app;
    }
}