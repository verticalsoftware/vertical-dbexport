// See https://aka.ms/new-console-template for more information

using Vertical.CommandLine;
using Vertical.DbExport.Infrastructure;
using Vertical.DbExport.Options;

Console.WriteLine("Hello, World!");

var cliConfig = CommandLineOptions.CreateConfiguration();
cliConfig.OnExecuteAsync(async options => await Runtime.RunAsync(options));

await CommandLineApplication.RunAsync(cliConfig, args);

