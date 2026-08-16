using System.Text.RegularExpressions;
using Wyag.Core.Exceptions;
using Wyag.Core.IO;
using Wyag.Core.Refs;

namespace Wyag.Core.Objects;

public sealed partial class GitObjectResolver(IFileSystem fs, IRefStore rs, IObjectStore os)
    : IObjectResolver
{

    [GeneratedRegex("^[0-9A-Fa-f]{4,40}$")]
    private static partial Regex HashPattern();

    public string Find(GitRepository repo, string name, string? format = null, bool follow = true)
    {
        var candidates = Resolve(repo, name);

        if (candidates.Count == 0)
            throw new GitException($"No such reference {name}");

        if (candidates.Count > 1)
            throw new GitException($"Ambiguous reference {name}: Candidates are:\n - {string.Join("\n - ", candidates)}.");

        var sha = candidates[0];

        if (format is null)
            return sha;

        while (true)
        {
            var obj = os.Read(repo, sha);

            if (obj.Format == format)
                return sha;

            if (!follow)
                throw new GitException($"No ojbect of type {format} found for {name}.");

            sha = obj switch
            {
                GitTag tag => tag.Kvlm.GetSingle("object")
                    ?? throw new GitException($"Tag {sha} has no object field."),
                GitCommit commit when format == "tree" => commit.Kvlm.GetSingle("tree")
                    ?? throw new GitException($"Commit {sha} has no tree field."),
                _ => throw new GitException($"No object of type {format} found for {name}."),
            };
        }
    }

    private List<string> Resolve(GitRepository repo, string name)
    {
        var candidates = new List<string>();

        if (string.IsNullOrWhiteSpace(name))
            return candidates;

        if (name == "HEAD")
        {
            var head = rs.Resolve(repo, "HEAD");
            if (head is not null)
                candidates.Add(head);
            return candidates;
        }

        if (HashPattern().IsMatch(name))
        {
            var lowered = name.ToLowerInvariant();
            var prefix = lowered[..2];
            var objectsDir = repo.Dir(mkdir: false, "objects", prefix);

            if (objectsDir is not null)
            {
                var remainder = lowered[2..];
                foreach (var file in fs.EnumerateFiles(objectsDir))
                {
                    var fileName = fs.GetFileName(file)!;
                    if (fileName.StartsWith(remainder, StringComparison.Ordinal))
                        candidates.Add(prefix + fileName);
                }
            }
        }

        if (rs.Resolve(repo, $"refs/tags/{name}") is { } asTag)
            candidates.Add(asTag);

        if (rs.Resolve(repo, $"refs/heads/{name}") is { } asBranch)
            candidates.Add(asBranch);

        if (rs.Resolve(repo, $"refs/remotes/{name}") is { } asRemoteBranch)
            candidates.Add(asRemoteBranch);

        return candidates;
    }
}
