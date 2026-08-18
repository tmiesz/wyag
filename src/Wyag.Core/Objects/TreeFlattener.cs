namespace Wyag.Core.Objects;

public static class TreeFlattener
{
    public static Dictionary<string, string> Flatten(
            GitRepository repo, IObjectStore os, string treeSha, string prefix = "")
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        var tree = (GitTree)os.Read(repo, treeSha);

        foreach (var leaf in tree.Items)
        {
            var fullPath = prefix.Length > 0 ? $"{prefix}/{leaf.Path}" : leaf.Path;

            if (leaf.Mode.StartsWith("04", StringComparison.Ordinal))
            {
                foreach (var (path, sha) in Flatten(repo, os, leaf.Sha, fullPath))
                    result[path] = sha;
            }
            else
            {
                result[fullPath] = leaf.Sha;
            }
        }

        return result;
    }
}
