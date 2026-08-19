using Wyag.Core.Exceptions;
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

    public void Write(GitRepository repo, GitIndex index)
    {
        var path = repo.File(mkdir: true, "index")
            ?? throw new GitException("Could not resolve the index file's path");

        fs.WriteBytes(path, GitIndexSerializer.Serialize(index));
    }
}
