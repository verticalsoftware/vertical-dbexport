using System.Text.RegularExpressions;

namespace Vertical.DbExport;

public static class TokenResolver
{
    public static string Resolve(string input, ParameterDictionary parameters)
    {
        return Regex.Replace(input, @"\$\{(\w+)\}", match =>
        {
            var token = match.Groups[1].Value;

            if (parameters.TryGetValue(token, out var replacement))
                return replacement;

            if (!string.IsNullOrWhiteSpace(replacement = Environment.GetEnvironmentVariable(token)))
                return replacement;

            if (Enum.TryParse(token, out Environment.SpecialFolder specialFolder))
                return Environment.GetFolderPath(specialFolder);

            throw new ApplicationException($"Could not resolve parameter '${{{token}}}'");
        });
    }
}