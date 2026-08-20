using Wyag.Core.Exceptions;
using Wyag.Core.Index;
using Wyag.Core.IO;
using Wyag.Core.Refs;

namespace Wyag.Core.Objects;

public sealed class CommitService(
        IFileSystem fs,
        IObjectStore os,
        IIndexStore indexStore,
        ITreeBuilder tb,
        IRefStore rs,
        IBranchService bs,
        IObjectResolver or,
        IAuthorIdentityProvider aip) : ICommitService
{
    public string Commit(GitRepository repo, string message)
    {
        var index = indexStore.Read(repo);
        var treeSha = tb.BuildTreeFromIndex(repo, index);

        var commit = new GitCommit();
        commit.Kvlm.Add("tree", treeSha);

        var parentSha = TryResolveHead(repo);
        if (parentSha is not null)
            commit.Kvlm.Add("parent", parentSha);

        var identity = aip.GetIdentity(repo);
        var timestamp = GitTimestamp.Format(DateTimeOffset.Now);

        commit.Kvlm.Add("author", $"{identity} {timestamp}");
        commit.Kvlm.Add("committer", $"{identity} {timestamp}");
        commit.Kvlm.Message = message.EndsWith('\n') ? message : message + "\n";

        var commitSha = os.Write(commit, repo);

        var activeBranch = bs.GetActiveBranch(repo);
        if (activeBranch is not null)
        {
            rs.Create(repo, $"heads/{activeBranch}", commitSha);
        }
        else
        {
            var headPath = repo.File(mkdir: false, "HEAD")
                ?? throw new GitException("Could not resolve .git/HEAD path.");
            fs.WriteText(headPath, commitSha + "\n");
        }

        return commitSha;
    }

    private string? TryResolveHead(GitRepository repo)
    {
        try
        {
            return or.Find(repo, "HEAD");
        }
        catch (GitException)
        {
            return null;
        }
    }
}
