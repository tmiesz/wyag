namespace Wyag.Core.Refs;

public sealed class BranchService(IRefStore refStore) : IBranchService
{
    private const string HeadsPrefix = "ref: refs/heads/";

    public string? GetActiveBranch(GitRepository repo)
    {
        var head = refStore.ReadRaw(repo, "HEAD");
        
        return head is not null && head.StartsWith(HeadsPrefix, StringComparison.Ordinal)
            ? head[HeadsPrefix.Length..]
            : null;
    }
}
