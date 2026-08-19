namespace Wyag.Core.Index;

public interface IStagingService
{
    void Remove(GitRepository repo, IReadOnlyList<string> paths, bool deleteFromDisk = true, bool skipMissing = false);
    void Add(GitRepository repo, IReadOnlyList<string> paths);
}
