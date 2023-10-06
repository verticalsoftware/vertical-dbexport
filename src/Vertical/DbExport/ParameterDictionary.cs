using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Vertical.DbExport;

public sealed class ParameterDictionary
{
    private readonly ImmutableList<IReadOnlyDictionary<string, string>> _sources;

    private ParameterDictionary(ImmutableList<IReadOnlyDictionary<string, string>> sources) => _sources = sources;

    public static ParameterDictionary Create(IReadOnlyDictionary<string, string> dictionary) =>
        new ParameterDictionary(ImmutableList<IReadOnlyDictionary<string, string>>.Empty.Add(dictionary));

    public ParameterDictionary Merge(IReadOnlyDictionary<string, string> dictionary) =>
        new ParameterDictionary(_sources.Insert(0, dictionary));

    public string this[string key] => TryGetValue(key, out var value)
        ? value
        : throw new KeyNotFoundException($"Key '{key}' is not present in the collection");

    public bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
    {
        foreach (var source in _sources)
        {
            if (source.TryGetValue(key, out value)) 
                return true;
        }

        value = default!;
        return false;
    }
}