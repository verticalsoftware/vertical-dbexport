using System.Collections.Concurrent;
using System.Reflection;

namespace Vertical.DbExport.Resources;

public static class StringResourceReader
{
    private static readonly ConcurrentDictionary<string, string> CachedValues = new();
    
    public static string GetResource(string path, string key)
    {
        return CachedValues.GetOrAdd($"{path}+{key}", _ => LoadResource(path, key));
    }

    private static string LoadResource(string path, string key)
    {
        var assemblyPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
        var resolvedPath = Path.Combine(
            assemblyPath,
            "Resources",
            path);
        
        var fileContent = File.ReadAllLines(resolvedPath);
        return string.Join(Environment.NewLine, ParseResourceContent(fileContent, key));
    }

    private static IEnumerable<string> ParseResourceContent(string[] fileContent, string key)
    {
        var marker = $"[{key}]";

        return fileContent
            .SkipWhile(str => str.Trim() != marker)
            .Skip(1)
            .TakeWhile(str => !string.IsNullOrWhiteSpace(str));
    }
}