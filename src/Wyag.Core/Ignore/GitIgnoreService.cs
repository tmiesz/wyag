using System.Text;
using Wyag.Core.Index;
using Wyag.Core.IO;
using Wyag.Core.Objects;

namespace Wyag.Core.Ignore;

public sealed class GitIgnoreService(IFileSystem fs, IObjectStore os, IIndexStore indexStore)
    : IGitIgnoreService
{
    public bool IsIgnored(GitIgnoreRules rules, string path)
    {
        if (GitPath.IsRooted(path))
            throw new Exception("This function requires path to be relative to the repository's root.");

        var scoped = CheckScoped(rules.Scoped, path);
        return scoped ?? CheckAbsolute(rules.Absolute, path);
    }

    public GitIgnoreRules Load(GitRepository repo)
    {
        var rules = new GitIgnoreRules();

        var excludePath = repo.File(mkdir: false, "info", "exclude");
        if (excludePath is not null && fs.FileExists(excludePath))
        {
            rules.Absolute.Add(GitIgnoreLineParser.ParseLines(fs.ReadText(excludePath).Split('\n')));
        }

        var index = indexStore.Read(repo);
        foreach (var entry in index.Entries)
        {
            if (entry.Name != ".gitignore" && !entry.Name.EndsWith("/.gitignore", StringComparison.Ordinal))
                continue;

            var directory = GitPath.GetParentDirectory(entry.Name);
            var blob = (GitBlob)os.Read(repo, entry.Sha);
            var lines = Encoding.UTF8.GetString(blob.BlobData).Split('\n');
            rules.Scoped[directory] = GitIgnoreLineParser.ParseLines(lines);
        }

        return rules;
    }

    private static bool CheckAbsolute(List<List<GitIgnoreRule>> rulesets, string path)
    {
        foreach (var ruleset in rulesets)
        {
            if (CheckRuleset(ruleset, path) is { } result)
                return result;
        }
        return false;
    }

    private static bool? CheckRuleset(List<GitIgnoreRule> rules, string path)
    {
        bool? result = null;
        foreach (var rule in rules)
        {
            if (GlobMatcher.IsMatch(path, rule.Pattern))
                result = rule.Exclude;
        }
        return result;
    }

    private static bool? CheckScoped(Dictionary<string, List<GitIgnoreRule>> scoped, string path)
    {
        var directory = GitPath.GetParentDirectory(path);

        while (true)
        {
            if (scoped.TryGetValue(directory, out var rules) && CheckRuleset(rules, path) is { } result)
                return result;

            if (directory.Length == 0)
                return null;

            directory = GitPath.GetParentDirectory(directory);
        }
    }
}
