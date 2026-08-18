namespace Wyag.Core.Refs;

public interface IBranchService
{
    string? GetActiveBranch(GitRepository repo);
}
