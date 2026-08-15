using System.Text;

namespace Wyag.Core.Objects;

public sealed class GitTreeSerializer
{
    public static byte[] Serialize(IReadOnlyList<GitTreeLeaf> items)
    {
        var sorted = items.OrderBy(SortKey, StringComparer.Ordinal);

        using var buffer = new MemoryStream();
        foreach (var leaf in sorted)
        {
            buffer.Write(Encoding.ASCII.GetBytes(leaf.Mode));
            buffer.WriteByte((byte)' ');
            buffer.Write(Encoding.UTF8.GetBytes(leaf.Path));
            buffer.WriteByte(0);
            buffer.Write(Convert.FromHexString(leaf.Sha));
        }

        return buffer.ToArray();
    }

    private static string SortKey(GitTreeLeaf leaf) =>
        leaf.Mode.StartsWith('4') ? leaf.Path + "/" : leaf.Path;
}
