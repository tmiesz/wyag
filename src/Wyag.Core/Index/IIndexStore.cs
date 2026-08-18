namespace Wyag.Core.Index;

public interface IIndexStore
{
    GitIndex Read(GitRepository repo);
}
