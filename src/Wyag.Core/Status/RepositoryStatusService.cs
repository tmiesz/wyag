using Wyag.Core.Ignore;
using Wyag.Core.Index;
using Wyag.Core.IO;
using Wyag.Core.Objects;
using Wyag.Core.Refs;

namespace Wyag.Core.Status;

public sealed class RepositoryStatusService(
        IFileSystem fs,
        IObjectStore os,
        IObjectResolver or,
        IIndexStore indexStore,
        IBranchService bs,
        IGitIgnoreService gis) : IRepositoryStatusService
{
    public RepositoryStatus GetStatus(GitRepository repo)
    {
        var index = indexStore.Read(repo);

        var activeBranch = bs.GetActiveBranch(repo);
        var detachedHeadSha = activeBranch is null ? or.Find(repo, "HEAD") : null;

        var staged = ComputeStagedChanges(repo, index);
        var tracked = new HashSet<string>(StringComparer.Ordinal);
        var unstaged = ComputeUnstagedChanges(repo, index, tracked);
        var untracked = new List<string>();
        var rules = gis.Load(repo);
        CollectUntracked(repo, repo.Worktree, prefix: "", tracked, rules, untracked);

        return new RepositoryStatus(activeBranch, detachedHeadSha, staged, unstaged, untracked);
    }

    private List<StatusChange> ComputeStagedChanges(GitRepository repo, GitIndex index)
    {
        var changes = new List<StatusChange>();

        var headTreeSha = or.Find(repo, "HEAD", format: "tree");
        var head = TreeFlattener.Flatten(repo, os, headTreeSha);
        var staged = index.Entries.ToDictionary(e => e.Name, e => e.Sha, StringComparer.Ordinal);

        foreach (var (path, sha) in head)
        {
            if (staged.TryGetValue(path, out var stagedSha))
            {
                if (stagedSha != sha)
                    changes.Add(new StatusChange(path, ChangeKind.Modified));
            }
            else
            {
                changes.Add(new StatusChange(path, ChangeKind.Deleted));
            }
        }

        foreach (var path in staged.Keys.Where(path => !head.ContainsKey(path)))
        {
            changes.Add(new StatusChange(path, ChangeKind.Added));
        }

        return changes;
    }

    private List<StatusChange> ComputeUnstagedChanges(GitRepository repo, GitIndex index, HashSet<string> tracked)
    {
        var changes = new List<StatusChange>();

        foreach (var entry in index.Entries)
        {
            tracked.Add(entry.Name);
            var fullPath = fs.Combine(repo.Worktree, entry.Name);

            if (!fs.FileExists(fullPath))
            {
                changes.Add(new StatusChange(entry.Name, ChangeKind.Deleted));
                continue;
            }

            var currentSha = os.Write(new GitBlob(fs.ReadBytes(fullPath)), repo: null);
            if (currentSha != entry.Sha)
                changes.Add(new StatusChange(entry.Name, ChangeKind.Modified));
        }

        return changes;
    }

    private void CollectUntracked(GitRepository repo, string directory, string prefix, HashSet<string> tracked, GitIgnoreRules rules, List<string> results)
    {
        foreach (var subDirectory in fs.EnumerateDirectories(directory))
        {
            var name = fs.GetFileName(subDirectory);

            if (prefix.Length == 0 && name == ".git")
                continue;

            var relativePath = prefix.Length > 0 ? $"{prefix}/{name}" : name!;
            if (gis.IsIgnored(rules, relativePath))
                continue;

            CollectUntracked(repo, subDirectory, relativePath, tracked, rules, results);
        }

        foreach (var file in fs.EnumerateFiles(directory))
        {
            var name = fs.GetFileName(file);
            var relativePath = prefix.Length > 0 ? $"{prefix}/{name}" : name!;

            if (tracked.Contains(relativePath) || gis.IsIgnored(rules, relativePath))
                continue;

            results.Add(relativePath);
        }
    }

}
