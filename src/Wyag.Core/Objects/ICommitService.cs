namespace Wyag.Core.Objects;

public interface ICommitService
{
    string Commit(GitRepository repo, string message);
}
