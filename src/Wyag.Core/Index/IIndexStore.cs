namespace Wyag.Core.Index;

public interface IIndexStore
{
    GitIndex Read(GitRepository repo);
    void Write(GitRepository repo, GitIndex index);
}
