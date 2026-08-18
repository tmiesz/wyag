namespace Wyag.Core.Index;

public sealed class GitIndex
{
    public uint Version { get; init; } = 2;
    public List<GitIndexEntry> Entries { get; init; } = [];
}
