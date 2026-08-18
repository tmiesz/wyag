namespace Wyag.Core.Ignore;

public interface IGitIgnoreService
{
    GitIgnoreRules Load(GitRepository repo);
    bool IsIgnored(GitIgnoreRules rules, string path);
}
