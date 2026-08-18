namespace Wyag.Core.Status;

public enum ChangeKind { Added, Modified, Deleted }

public sealed record StatusChange(string Path, ChangeKind Kind);

public sealed record RepositoryStatus(
        string? ActiveBranch,
        string? DetachedHeadSha,
        IReadOnlyList<StatusChange> StagedChanges,
        IReadOnlyList<StatusChange> UnstagedChanges,
        IReadOnlyList<string> UntrackedFiles);
