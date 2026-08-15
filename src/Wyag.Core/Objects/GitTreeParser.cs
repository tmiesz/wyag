using System.Text;
using Wyag.Core.Exceptions;

namespace Wyag.Core.Objects;

public static class GitTreeParser
{
    public static List<GitTreeLeaf> Parse(byte[] raw)
    {
        var entries = new List<GitTreeLeaf>();
        var position = 0;

        while (position < raw.Length)
        {
            (position, var leaf) = ParseOne(raw, position);
            entries.Add(leaf);
        }

        return entries;
    }

    private static (int NextPosition, GitTreeLeaf leaf) ParseOne(byte[] raw, int start)
    {
        var spaceIndex = IndexOf(raw, (byte)' ', start);
        var modeLength = spaceIndex - start;

        if (modeLength is not (5 or 6))
            throw new GitException($"Malformed tree entry: unexpected mode length {modeLength}");

        var mode = Encoding.ASCII.GetString(raw, start, modeLength);
        if (mode.Length == 5)
            mode = "0" + mode;

        var nullIndex = IndexOf(raw, 0, spaceIndex);
        var path = Encoding.UTF8.GetString(raw, spaceIndex + 1, nullIndex - spaceIndex - 1);

        var shaBytes = raw[(nullIndex + 1)..(nullIndex + 21)];
        var sha = Convert.ToHexString(shaBytes).ToLowerInvariant();

        return (nullIndex + 21, new GitTreeLeaf(mode, path, sha));
    }

    private static int IndexOf(byte[] data, byte value, int start)
    {
        for (var i = start; i < data.Length; i++)
        {
            if (data[i] == value) return i;
        }

        return -1;
    }
}
