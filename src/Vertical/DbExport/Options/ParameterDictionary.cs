using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Vertical.DbExport.Options;

public sealed class ParameterDictionary
{
    private readonly IReadOnlyDictionary<string, string> _dictionary;

    private ParameterDictionary(IReadOnlyDictionary<string, string> dictionary)
    {
        _dictionary = dictionary;
    }

    public static ParameterDictionary Create(params IReadOnlyDictionary<string, string>[] sources)
    {
        var dictionary = new Dictionary<string, string>();

        foreach (var (key, value) in sources.SelectMany(kv => kv))
        {
            dictionary[key] = value;
        }

        return new ParameterDictionary(dictionary);
    }

    public string this[string key] => _dictionary[key];

    public bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
    {
        return _dictionary.TryGetValue(key, out value);
    }
}