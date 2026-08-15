using Wyag.Core.Exceptions;
using Wyag.Core.IO;

namespace Wyag.Core.Refs;

public sealed class RefStore(IFileSystem fs) : IRefStore
{
    public void Create(GitRepository repo, string refName, string sha)
    {
        var path = repo.File(mkdir: false, $"refs/{refName}")
            ?? throw new GitException($"Could not resolve path for ref {refName}.");

        fs.WriteText(path, sha + "\n");
    }

    public SortedDictionary<string, object> List(GitRepository repo, string? path = null)
    {
        path ??= repo.Dir(mkdir: false, "refs")
            ?? throw new GitException("Repository has no refs directory.");

        var result = new SortedDictionary<string, object>(StringComparer.Ordinal);

        var entries = fs.EnumerateDirectories(path).Concat(fs.EnumerateFiles(path));

        foreach (var entryPath in entries)
        {
            var name = fs.GetFileName(entryPath)!;

            result[name] = fs.DirectoryExists(entryPath)
                ? List(repo, entryPath)
                : Resolve(repo, entryPath)!;
        }

        return result;
    }

    public string? ReadRaw(GitRepository repo, string refPath)
    {
        var path = repo.Path(refPath);

        return fs.FileExists(path) ? fs.ReadText(path).TrimEnd('\n') : null;
    }

    public string? Resolve(GitRepository repo, string refPath)
    {
        var data = ReadRaw(repo, refPath);
        if (data is null)
            return null;

        return data.StartsWith("ref: ", StringComparison.Ordinal)
            ? Resolve(repo, data[5..])
            : data;
    }
}
