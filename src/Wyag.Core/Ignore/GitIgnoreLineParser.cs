namespace Wyag.Core.Ignore;

public static class GitIgnoreLineParser
{
    public static GitIgnoreRule? ParseLine(string rawLine)
    {
        var line = rawLine.Trim();

        if(line.Length == 0 || line[0] == '#')
            return null;

        if (line[0] == '!')
            return new GitIgnoreRule(line[1..], Exclude: false);

        if (line[0] == '\\')
            return new GitIgnoreRule(line[1..], Exclude: true);

        return new GitIgnoreRule(line, Exclude: true);
    }

    public static List<GitIgnoreRule> ParseLines(IEnumerable<string> lines) =>
        [.. lines.Select(ParseLine).Where(rule => rule is not null).Select(rule => rule!)];
}
