using Wyag.Core.IO;

namespace Wyag.Core.Index;

public sealed class IndexStore(IFileSystem fs) : IIndexStore
{
    public GitIndex Read(GitRepository repo)
    {
        var path = repo.File(mkdir: false, "index");

        if (path is null || !fs.FileExists(path))
            return new GitIndex();

        return GitIndexParser.Parse(fs.ReadBytes(path));
    }
}
