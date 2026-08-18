namespace Wyag.Core.Ignore;

public sealed class GitIgnoreRules
{
    public List<List<GitIgnoreRule>> Absolute { get; } = [];

    public Dictionary<string, List<GitIgnoreRule>> Scoped { get; } = new(StringComparer.Ordinal);
}
