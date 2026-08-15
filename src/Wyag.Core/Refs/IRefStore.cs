namespace Wyag.Core.Refs;

/// <summary>
/// Reads, lists, resolves and creates Git references.
/// </summary>
public interface IRefStore
{
    string? ReadRaw(GitRepository repo, string refPath);

    string? Resolve(GitRepository repo, string refPath);

    SortedDictionary<string, object> List(GitRepository repo, string? path = null);

    void Create(GitRepository repo, string refName, string sha);
}
