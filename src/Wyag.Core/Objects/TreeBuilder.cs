using Wyag.Core.Exceptions;
using Wyag.Core.Index;

namespace Wyag.Core.Objects;

public sealed class TreeBuilder(IObjectStore os) : ITreeBuilder
{
    public string BuildTreeFromIndex(GitRepository repo, GitIndex index)
    {
        var contents = new Dictionary<string, List<(string Name, string Sha, bool IsTree)>>(StringComparer.Ordinal)
        {
            [""] = []
        };

        foreach (var entry in index.Entries)
        {
            var directory = GitPath.GetParentDirectory(entry.Name);

            var current = directory;
            while (current.Length > 0 && !contents.ContainsKey(current))
            {
                contents[current] = [];
                current = GitPath.GetParentDirectory(current);
            }

            contents[directory].Add((GitPath.GetFileName(entry.Name), entry.Sha, IsTree: false));
        }

        var directoriesDeepestFirst = contents.Keys.OrderByDescending(d => d.Length).ToList();
        string? rootSha = null;

        foreach (var directory in directoriesDeepestFirst)
        {
            var tree = new GitTree();

            foreach (var (name, sha, isTree) in contents[directory])
            {
                tree.Items.Add(new GitTreeLeaf(isTree ? "040000" : "100644", name, sha));
            }

            var treeSha = os.Write(tree, repo);

            if (directory.Length == 0)
            {
                rootSha = treeSha;
            }
            else
            {
                var parent = GitPath.GetParentDirectory(directory);
                var name = GitPath.GetFileName(directory);
                contents[parent].Add((name, treeSha, IsTree: true));
            }
        }
        return rootSha ?? throw new GitException("Cannot build a tree from an empty index.");
    }
}
